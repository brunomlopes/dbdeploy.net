using System;
using System.Collections.Generic;
using Net.Sf.Dbdeploy.Database;
using Net.Sf.Dbdeploy.Scripts;

namespace Net.Sf.Dbdeploy
{
    public class Controller
    {
        private readonly DatabaseSchemaVersionManager schemaVersion;
        private readonly ChangeScriptExecuter changeScriptExecuter;
        private readonly ChangeScriptRepository changeScriptRepository;
        private readonly PrettyPrinter prettyPrinter = new PrettyPrinter();
        private readonly List<ChangeScript> doChangeScripts;
        private List<ChangeScript> undoChangeScripts;
        private readonly List<int> appliedChanges;
        private List<int> changesToApply;

        public Controller(DatabaseSchemaVersionManager schemaVersion,
                          ChangeScriptRepository changeScriptRepository,
                          ChangeScriptExecuter changeScriptExecuter)
        {
            this.schemaVersion = schemaVersion;
            this.changeScriptRepository = changeScriptRepository;
            this.changeScriptExecuter = changeScriptExecuter;

            doChangeScripts = changeScriptRepository.GetOrderedListOfDoChangeScripts();
            appliedChanges = schemaVersion.GetAppliedChangeNumbers();
        }

        public void ProcessDoChangeScripts(int lastChangeToApply)
        {
            if (lastChangeToApply != int.MaxValue)
            {
                Info("Only applying changes up and including change script #" + lastChangeToApply);
            }

            Info("Changes currently applied to database:\n  " + prettyPrinter.Format(appliedChanges));
            Info("Scripts available:\n  " + prettyPrinter.formatChangeScriptList(doChangeScripts));

            changesToApply = new List<int>();

            LoopThruDoScripts(lastChangeToApply);

            Info("To be applied:\n  " + prettyPrinter.Format(changesToApply));
        }

        public void ProcessUndoChangeScripts(int lastChangeToApply)
        {
            undoChangeScripts = changeScriptRepository.GetOrderedListOfUndoChangeScripts();
            LoopThruUndoScripts(lastChangeToApply);
        }

        private static void Info(string text)
        {
            Console.Out.WriteLine(text);
        }

        private void LoopThruDoScripts(int lastChangeToApply)
        {
            foreach (ChangeScript changeScript in doChangeScripts)
            {
                int changeScriptId = changeScript.GetId();

                if (changeScriptId <= lastChangeToApply && !appliedChanges.Contains(changeScriptId))
                {
                    changesToApply.Add(changeScriptId);

                    String sql = schemaVersion.GenerateDoDeltaFragmentHeader(changeScript);
                    changeScriptExecuter.ApplyDeltaFragmentHeaderOrFooterSql(sql);

                    changeScriptExecuter.ApplyChangeDoScript(changeScript);

                    sql = schemaVersion.GenerateDoDeltaFragmentFooter(changeScript);
                    changeScriptExecuter.ApplyDeltaFragmentHeaderOrFooterSql(sql);
                }
            }
        }

        private void LoopThruUndoScripts(int lastChangeToApply)
        {
            foreach (ChangeScript changeScript in undoChangeScripts)
            {
                int changeScriptId = changeScript.GetId();

                if (changeScriptId <= lastChangeToApply && !appliedChanges.Contains(changeScriptId))
                {
                    changeScriptExecuter.ApplyChangeUndoScript(changeScript);

                    String sql = schemaVersion.GenerateUndoDeltaFragmentFooter(changeScript);
                    changeScriptExecuter.ApplyDeltaFragmentHeaderOrFooterSql(sql);
                }
            }
        }
    }
}