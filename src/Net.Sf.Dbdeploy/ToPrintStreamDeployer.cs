using System;
using System.Collections.Generic;
using System.IO;
using Net.Sf.Dbdeploy.Database;
using Net.Sf.Dbdeploy.Scripts;

namespace Net.Sf.Dbdeploy
{
    public class ToPrintStreamDeployer
    {
        private readonly DirectoryInfo dir;
        private readonly TextWriter doOutputPrintStream;
        private readonly TextWriter undoOutputPrintStream;
        private readonly DatabaseSchemaVersionManager schemaManager;
        private readonly DbmsSyntax dbmsSyntax;

        public ToPrintStreamDeployer(DatabaseSchemaVersionManager schemaManager, DirectoryInfo dir,
                                     TextWriter outputPrintStream,
                                     DbmsSyntax dbmsSyntax, TextWriter undoOutputPrintStream)
        {
            this.schemaManager = schemaManager;
            this.dir = dir;
            doOutputPrintStream = outputPrintStream;
            this.dbmsSyntax = dbmsSyntax;
            this.undoOutputPrintStream = undoOutputPrintStream;
        }

        public void DoDeploy(int lastChangeToApply)
        {
            Console.Out.WriteLine("dbdeploy v2.12");

            List<ChangeScript> changeScripts = new DirectoryScanner().GetChangeScriptsForDirectory(dir);
            ChangeScriptRepository repository = new ChangeScriptRepository(changeScripts);
            List<int> appliedChanges = schemaManager.GetAppliedChangeNumbers();

            GenerateChangeScripts(repository, lastChangeToApply, appliedChanges);
            if (undoOutputPrintStream != null)
            {
                GenerateUndoChangeScripts(repository, lastChangeToApply, appliedChanges);
            }
        }

        private void GenerateChangeScripts(ChangeScriptRepository repository, int lastChangeToApply, List<int> appliedChanges)
        {
            ChangeScriptExecuter doScriptExecuter = new ChangeScriptExecuter(doOutputPrintStream, dbmsSyntax);
            Controller doController = new Controller(schemaManager, repository, doScriptExecuter);
            doController.ProcessDoChangeScripts(lastChangeToApply, appliedChanges);
            doOutputPrintStream.Flush();
        }

        private void GenerateUndoChangeScripts(ChangeScriptRepository repository, int lastChangeToApply, List<int> appliedChanges)
        {
            ChangeScriptExecuter undoScriptExecuter = new ChangeScriptExecuter(undoOutputPrintStream, dbmsSyntax);
            Controller undoController = new Controller(schemaManager, repository, undoScriptExecuter);
            undoController.ProcessUndoChangeScripts(lastChangeToApply, appliedChanges);
            undoOutputPrintStream.Flush();
        }
    }
}