using System.Collections.Generic;
using System.IO;
using Net.Sf.Dbdeploy.Scripts;

namespace Net.Sf.Dbdeploy
{
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

        public void ProcessChangeScripts(int? lastChangeToApply)
        {
            if (lastChangeToApply.HasValue)
            {
                Info("Only applying changes up and including change script #" + lastChangeToApply);
            }

            var scripts = this.availableChangeScriptsProvider.GetAvailableChangeScripts();
            var applied = this.appliedChangesProvider.GetAppliedChanges();
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

        private void LogStatus(ICollection<ChangeScript> scripts, ICollection<int> applied, ICollection<ChangeScript> toApply)
        {
            Info("Changes currently applied to database:\n  " + prettyPrinter.Format(applied));
            Info("Scripts available:\n  " + prettyPrinter.FormatChangeScriptList(scripts));
            Info("To be applied:\n  " + prettyPrinter.FormatChangeScriptList(toApply));
        }

        private ICollection<ChangeScript> IdentifyChangesToApply(int? lastChangeToApply, IEnumerable<ChangeScript> scripts, ICollection<int> applied)
        {
            var result = new List<ChangeScript>();

            foreach (ChangeScript script in scripts)
            {
                if (lastChangeToApply.HasValue && script.GetId() > lastChangeToApply.Value)
                    break;

                if (!applied.Contains(script.GetId()))
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