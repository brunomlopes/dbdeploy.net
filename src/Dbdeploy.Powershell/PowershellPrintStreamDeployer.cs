using System.Collections.Generic;
using System.IO;
using Net.Sf.Dbdeploy;
using Net.Sf.Dbdeploy.Database;
using Net.Sf.Dbdeploy.Scripts;

namespace Dbdeploy.Powershell
{
    public class PowershellPrintStreamDeployer
    {
        private readonly DirectoryInfo dir;
        private readonly TextWriter doOutputPrintStream;
        private readonly TextWriter undoOutputPrintStream;
        private readonly TextWriter infoPrintStream;
        private readonly DatabaseSchemaVersionManager schemaManager;
        private readonly IDbmsSyntax dbmsSyntax;
        private readonly bool useTransaction;
        private ChangeScriptRepository repository;

        public PowershellPrintStreamDeployer(DatabaseSchemaVersionManager schemaManager, ChangeScriptRepository repository,
                                             TextWriter outputPrintStream,
                                             IDbmsSyntax dbmsSyntax, bool useTransaction,
                                             TextWriter undoOutputPrintStream,
                                             TextWriter infoPrintStream)
        {   
            this.schemaManager = schemaManager;
            this.dir = dir;
            this.doOutputPrintStream = outputPrintStream;
            this.dbmsSyntax = dbmsSyntax;
            this.useTransaction = useTransaction;
            this.undoOutputPrintStream = undoOutputPrintStream;
            this.infoPrintStream = infoPrintStream;
            this.repository = repository;

        }

        public void DoDeploy(int lastChangeToApply, TextWriter infoTextWriter)
        {
            List<int> appliedChanges = schemaManager.GetAppliedChangeNumbers();

            GenerateChangeScripts(repository, lastChangeToApply, appliedChanges);
            if (undoOutputPrintStream != null)
            {
                GenerateUndoChangeScripts(repository, lastChangeToApply, appliedChanges);
            }
        }

        private void GenerateChangeScripts(ChangeScriptRepository repository, int lastChangeToApply, List<int> appliedChanges)
        {
            ChangeScriptExecuter doScriptExecuter = new ChangeScriptExecuter(doOutputPrintStream, dbmsSyntax, useTransaction);
            Controller doController = new Controller(schemaManager, repository, doScriptExecuter, infoPrintStream);
            doController.ProcessDoChangeScripts(lastChangeToApply, appliedChanges);
            doOutputPrintStream.Flush();
        }

        private void GenerateUndoChangeScripts(ChangeScriptRepository repository, int lastChangeToApply, List<int> appliedChanges)
        {
            ChangeScriptExecuter undoScriptExecuter = new ChangeScriptExecuter(undoOutputPrintStream, dbmsSyntax, useTransaction);
            Controller undoController = new Controller(schemaManager, repository, undoScriptExecuter, infoPrintStream);
            undoController.ProcessUndoChangeScripts(lastChangeToApply, appliedChanges);
            undoOutputPrintStream.Flush();
        }
    }
}