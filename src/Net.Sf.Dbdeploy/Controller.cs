using Net.Sf.Dbdeploy.Appliers;

namespace Net.Sf.Dbdeploy
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    using Net.Sf.Dbdeploy.Database;
    using Net.Sf.Dbdeploy.Exceptions;
    using Net.Sf.Dbdeploy.Scripts;

    /// <summary>
    /// Primary controller for executing DbDeploy.
    /// </summary>
    public class Controller
    {
        /// <summary>
        /// The writer for displaying information to the user.
        /// </summary>
        private static TextWriter infoWriter;

        private readonly IRepositorioScripts repositorioScripts;

        private readonly IAvailableChangeScriptsProvider availableChangeScriptsProvider;

        /// <summary>
        /// The applied changes provider.
        /// </summary>
        private readonly IAppliedChangesProvider appliedChangesProvider;

        /// <summary>
        /// The applier to upgrade the database.
        /// </summary>
        private readonly IChangeScriptApplier doApplier;

        /// <summary>
        /// The applier to undo changes to the database.
        /// </summary>
        private readonly IChangeScriptApplier undoApplier;

        private readonly bool createChangeLogTable;

        /// <summary>
        /// The printer for handling the output of the script changes.
        /// </summary>
        private readonly PrettyPrinter prettyPrinter = new PrettyPrinter();

        /// <summary>
        /// Initializes a new instance of the <see cref="Controller" /> class.
        /// </summary>
        /// <param name="availableChangeScriptsProvider">The available change scripts provider.</param>
        /// <param name="appliedChangesProvider">The applied changes provider.</param>
        /// <param name="doApplier">The do applier.</param>
        /// <param name="undoApplier">The undo applier.</param>
        /// <param name="createChangeLogTable">Whether the change log table should be created or not.</param>
        /// <param name="infoTextWriter">The info text writer.</param>
        /// <param name="repositorioScripts"></param>
        public Controller(
            IRepositorioScripts repositorioScripts,
            IAppliedChangesProvider appliedChangesProvider,
            IChangeScriptApplier doApplier,
            IChangeScriptApplier undoApplier,
            bool createChangeLogTable,
            TextWriter infoTextWriter)
        {
            this.doApplier = doApplier;
            this.undoApplier = undoApplier;
            this.createChangeLogTable = createChangeLogTable;

            this.appliedChangesProvider = appliedChangesProvider;
            
            infoWriter = infoTextWriter;
            this.repositorioScripts = repositorioScripts;
        }

        /// <summary>
        /// Processes the change scripts.
        /// </summary>
        /// <param name="lastChangeToApply">The last change to apply.</param>
        /// <param name="forceUpdate">if set to <c>true</c> any previously failed scripts will be retried.</param>
        public void ProcessChangeScripts(UniqueChange lastChangeToApply, bool forceUpdate = false)
        {
            if (lastChangeToApply != null)
            {
                Info("\nOnly applying changes up to and including change script '{0}'.\n", lastChangeToApply);
            }
            
            var applied = repositorioScripts.ObterScriptsAplicados();

            // If force update is not set, than if there are any previous script runs that failed it should stop.
            if (!forceUpdate)
            {
                this.CheckForFailedScripts(applied);
            }

            var scripts = repositorioScripts.ObterTodosOsScripts();
            var toApply = repositorioScripts.ObterScriptsPendenteExecucao(lastChangeToApply);

            this.LogStatus(scripts, applied, toApply);

            var includeChangeLogTable = this.createChangeLogTable && !this.appliedChangesProvider.ChangeLogTableExists();
            this.doApplier.Apply(toApply, includeChangeLogTable);

            if (this.undoApplier != null)
            {
                Info("Generating undo scripts...");

                var toUndoApply = new List<ChangeScript>(toApply);
                toUndoApply.Reverse();

                this.undoApplier.Apply(toUndoApply, false);
            }

            if (toApply.Any())
            {
                Info("All scripts applied successfully.");
            }
        }

        /// <summary>
        /// Writes out the specified info message.
        /// </summary>
        /// <param name="text">The text for the message.</param>
        /// <param name="args">The args to format into the message.</param>
        private static void Info(string text, params object[] args)
        {
            infoWriter.WriteLine(text, args);
        }

        /// <summary>
        /// Checks for failed scripts from previous runs.
        /// </summary>
        /// <param name="applied">The applied scripts from previous runs.</param>
        /// <exception cref="PriorFailedScriptException">Thrown if a previous run failed.</exception>
        private void CheckForFailedScripts(IEnumerable<ChangeEntry> applied)
        {
            var failedScript = applied.FirstOrDefault(a => a.Status == ScriptStatus.Failure);
            if (failedScript != null)
            {
                const string FailedMessage = @"
The script '{0}' failed to complete on a previous run. 
You must update the status to Resolved (2), or force updates.

Output from the previous run
----------------------------------------------------------
{1}";
                throw new PriorFailedScriptException(string.Format(CultureInfo.InvariantCulture, FailedMessage, failedScript, failedScript.Output));
            }
        }

        /// <summary>
        /// Logs the status of the scripts.
        /// </summary>
        /// <param name="scripts">The scripts.</param>
        /// <param name="applied">The applied.</param>
        /// <param name="toApply">To apply.</param>
        private void LogStatus(IEnumerable<ChangeScript> scripts, IEnumerable<ChangeEntry> applied, IEnumerable<ChangeScript> toApply)
        {
            Info(string.Empty);
            Info("Changes currently applied to database:\n" + this.prettyPrinter.Format(applied));
            Info("Scripts available:\n" + this.prettyPrinter.Format(scripts));
            Info("To be applied:\n" + this.prettyPrinter.Format(toApply));
            Info(string.Empty);
        }

        /// <summary>
        /// Identifies the changes to apply to the database.
        /// </summary>
        /// <param name="lastChangeToApply">The last change to apply.</param>
        /// <param name="scripts">The scripts.</param>
        /// <param name="applied">The applied changes.</param>
        /// <returns>List of changes to apply.</returns>
        private IList<ChangeScript> IdentifyChangesToApply(UniqueChange lastChangeToApply, IEnumerable<ChangeScript> scripts, IList<ChangeEntry> applied)
        {
            var changes = new List<ChangeScript>();

            // Re-run any scripts that have not been run, or are failed or resolved.
            // The check to exit on previous failure is done before this call.
            foreach (var script in scripts)
            {
                // If script has not been run yet, add it to the list.
                bool applyScript = false;
                var changeEntry = applied.FirstOrDefault(a => a.CompareTo(script) == 0);
                if (changeEntry == null)
                {
                    applyScript = true;
                }
                else
                {
                    // If the script has already been run check if it should be run again.
                    if (changeEntry.Status != ScriptStatus.Success)
                    {
                        // Assign the ID so the record can be updated.
                        script.ChangeId = changeEntry.ChangeId;
                        applyScript = true;
                    }
                }

                if (applyScript)
                {
                    // Just add script if there is no cap specified.
                    if (lastChangeToApply == null)
                    {
                        changes.Add(script);
                    }
                    else if (script.CompareTo(lastChangeToApply) <= 0)
                    {
                        // Script is less than last change to apply.
                        changes.Add(script);
                    }
                    else
                    {
                        // Stop adding scripts as last change to apply has been met.
                        break;
                    }
                }
            }

            return changes;
        }
    }
}