using System;
using System.Collections.Generic;
using Net.Sf.Dbdeploy.Database;
using Net.Sf.Dbdeploy.Scripts;

namespace Net.Sf.Dbdeploy
{
    public class Controller
    {
        private readonly DatabaseSchemaVersionManager schemaManager;
        private readonly ChangeScriptExecuter changeScriptExecuter;
        private readonly ChangeScriptRepository changeScriptRepository;
        private readonly PrettyPrinter prettyPrinter = new PrettyPrinter();

        public Controller(DatabaseSchemaVersionManager schemaManager,
                          ChangeScriptRepository changeScriptRepository,
                          ChangeScriptExecuter changeScriptExecuter)
        {
            this.schemaManager = schemaManager;
            this.changeScriptRepository = changeScriptRepository;
            this.changeScriptExecuter = changeScriptExecuter;
        }

        public void ProcessDoChangeScripts(int lastChangeToApply, List<int> appliedChanges)
        {
            if (lastChangeToApply != int.MaxValue)
            {
                Info("Only applying changes up and including change script #" + lastChangeToApply);
            }
            Info("Changes currently applied to database:\n  " + prettyPrinter.Format(appliedChanges));

            List<ChangeScript> doChangeScripts = changeScriptRepository.GetOrderedListOfDoChangeScripts();
            Info("Scripts available:\n  " + prettyPrinter.FormatChangeScriptList(doChangeScripts));

            changeScriptExecuter.ApplyDeltaFragmentHeaderOrFooterSql(schemaManager.GenerateVersionCheck());
            List<int> changesToApply = LoopThruDoScripts(lastChangeToApply, doChangeScripts, appliedChanges);
            Info("To be applied:\n  " + prettyPrinter.Format(changesToApply));
        }

        public void ProcessUndoChangeScripts(int lastChangeToApply, List<int> appliedChanges)
        {
            List<ChangeScript> undoChangeScripts = changeScriptRepository.GetOrderedListOfUndoChangeScripts();
            LoopThruUndoScripts(lastChangeToApply, undoChangeScripts, appliedChanges);
        }

        private List<int> LoopThruDoScripts(int lastChangeToApply, IEnumerable<ChangeScript> doChangeScripts, ICollection<int> appliedChanges)
        {
            List<int> changesToApply = new List<int>();
            foreach (ChangeScript changeScript in doChangeScripts)
            {
                int changeScriptId = changeScript.GetId();

                if (changeScriptId <= lastChangeToApply && !appliedChanges.Contains(changeScriptId))
                {
                    changesToApply.Add(changeScriptId);

                    changeScriptExecuter.ApplyDeltaFragmentHeaderOrFooterSql(schemaManager.GenerateDoDeltaFragmentHeader(changeScript));
                    changeScriptExecuter.ApplyChangeDoScript(changeScript);
                    changeScriptExecuter.ApplyDeltaFragmentHeaderOrFooterSql(schemaManager.GenerateDoDeltaFragmentFooter(changeScript));
                }
            }
            return changesToApply;
        }

        private void LoopThruUndoScripts(int lastChangeToApply, IEnumerable<ChangeScript> undoChangeScripts, ICollection<int> appliedChanges)
        {
            foreach (ChangeScript changeScript in undoChangeScripts)
            {
                int changeScriptId = changeScript.GetId();

                if (changeScriptId <= lastChangeToApply && !appliedChanges.Contains(changeScriptId))
                {
                    changeScriptExecuter.ApplyChangeUndoScript(changeScript);
                    changeScriptExecuter.ApplyDeltaFragmentHeaderOrFooterSql(schemaManager.GenerateUndoDeltaFragmentFooter(changeScript));
                }
            }
        }

        private static void Info(string text)
        {
            Console.Out.WriteLine(text);
        }
    }
}