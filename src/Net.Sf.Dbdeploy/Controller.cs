namespace Net.Sf.Dbdeploy
{
    using System;
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

        /// <summary>
        /// The available change scripts provider.
        /// </summary>
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
        /// <param name="infoTextWriter">The info text writer.</param>
        public Controller(
            IAvailableChangeScriptsProvider availableChangeScriptsProvider,
            IAppliedChangesProvider appliedChangesProvider,
            IChangeScriptApplier doApplier,
            IChangeScriptApplier undoApplier,
            TextWriter infoTextWriter)
        {
            this.doApplier = doApplier;
            this.undoApplier = undoApplier;

            this.appliedChangesProvider = appliedChangesProvider;

            this.availableChangeScriptsProvider = availableChangeScriptsProvider;
            
            infoWriter = infoTextWriter;
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
                Info("Only applying changes up and including change script " + lastChangeToApply);
            }

            // If force update is not set, than if there are any previous script runs that failed it should stop.
            var applied = this.appliedChangesProvider.GetAppliedChanges();
            if (!forceUpdate)
            {
                this.CheckForFailedScripts(applied);
            }

            var scripts = this.availableChangeScriptsProvider.GetAvailableChangeScripts();
            var toApply = this.IdentifyChangesToApply(lastChangeToApply, scripts, applied);

            this.LogStatus(scripts, applied, toApply);

            this.doApplier.Apply(toApply);

            if (this.undoApplier != null)
            {
                Info("Generating undo scripts...");

                var toUndoApply = new List<ChangeScript>(toApply);
                toUndoApply.Reverse();

                this.undoApplier.Apply(toUndoApply);
            }
        }

        /// <summary>
        /// Writes out the specified info message.
        /// </summary>
        /// <param name="text">The text for the message.</param>
        private static void Info(string text)
        {
            infoWriter.WriteLine(text);
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
                throw new PriorFailedScriptException(string.Format(CultureInfo.InvariantCulture, @"
The script '{0}' failed to complete on a previous run. 
You must update the status to Resolved (2), or force updates.

Ouput from the previous run
----------------------------------------------------------
{1}", failedScript, failedScript.Output));
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
            Info("Changes currently applied to database:\n  " + this.prettyPrinter.Format(applied));
            Info("Scripts available:\n  " + this.prettyPrinter.Format(scripts));
            Info("To be applied:\n  " + this.prettyPrinter.Format(toApply));
            Info(string.Empty);
        }

        /// <summary>
        /// Identifies the changes to apply to the database.
        /// </summary>
        /// <param name="lastChangeToApply">The last change to apply.</param>
        /// <param name="scripts">The scripts.</param>
        /// <param name="applied">The applied changes.</param>
        /// <returns>List of changes to apply.</returns>
        private IList<ChangeScript> IdentifyChangesToApply(UniqueChange lastChangeToApply, IEnumerable<ChangeScript> scripts, IEnumerable<ChangeEntry> applied)
        {
            // Re-run any scripts marked as resolved.
            var changes = scripts.Where(s => applied.All(a => !string.Equals(a.UniqueKey, s.UniqueKey, StringComparison.InvariantCultureIgnoreCase)));

            // Only apply up to last change if specified.
            if (lastChangeToApply != null)
            {
                changes = changes.Where(s => s.CompareTo(lastChangeToApply) <= 0);
            }

            return changes.ToList();
        }
    }
}