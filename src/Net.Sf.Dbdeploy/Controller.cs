using System.Collections.Generic;
using System.IO;
using System.Linq;
using Net.Sf.Dbdeploy.Scripts;
using Net.Sf.Dbdeploy.Database;

namespace Net.Sf.Dbdeploy
{
    using System.Globalization;

    using Net.Sf.Dbdeploy.Exceptions;

    public class Controller
    {
        private readonly IAvailableChangeScriptsProvider availableChangeScriptsProvider;
        private readonly IAppliedChangesProvider appliedChangesProvider;

        private readonly IChangeScriptApplier doApplier;
        private readonly IChangeScriptApplier undoApplier;

        private readonly PrettyPrinter prettyPrinter = new PrettyPrinter();
        
        private static TextWriter infoWriter;

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
        public void ProcessChangeScripts(int? lastChangeToApply, bool forceUpdate = false)
        {
            if (lastChangeToApply.HasValue)
            {
                Info("Only applying changes up and including change script #" + lastChangeToApply);
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

        private void CheckForFailedScripts(ICollection<ChangeEntry> applied)
        {
            var failedScript = applied.FirstOrDefault(a => a.Status == ScriptStatus.Failure);
            if (failedScript != null)
            {
                throw new PriorFailedScriptException(string.Format(CultureInfo.InvariantCulture, "The script '{0}' failed to complete on a previous run.\r\n{1}", failedScript, failedScript.Output));
            }
        }

        private void LogStatus(ICollection<ChangeScript> scripts, ICollection<ChangeEntry> applied, ICollection<ChangeScript> toApply)
        {
            Info("Changes currently applied to database:\n  " + prettyPrinter.Format(applied));
            Info("Scripts available:\n  " + prettyPrinter.Format(scripts));
            Info("To be applied:\n  " + prettyPrinter.Format(toApply));
        }

        private ICollection<ChangeScript> IdentifyChangesToApply(int? lastChangeToApply, IEnumerable<ChangeScript> scripts, ICollection<ChangeEntry> applied)
        {
            var result = new List<ChangeScript>();

            foreach (ChangeScript script in scripts)
            {
                if (lastChangeToApply.HasValue && script.ScriptNumber > lastChangeToApply.Value)
                    break;

                var appliedScript = applied.FirstOrDefault(a => a.ChangeId == script.ScriptNumber);

                if (applied.Any(a => a.ScriptNumber == script.ScriptNumber))
                {
                    result.Add(script);
                }
            }

            return result;
        }

        private static void Info(string text)
        {
            infoWriter.WriteLine(text);
        }
    }
}