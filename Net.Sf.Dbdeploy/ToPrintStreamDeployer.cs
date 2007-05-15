using System;
using System.IO;
using Net.Sf.Dbdeploy.Database;
using Net.Sf.Dbdeploy.Scripts;

namespace Net.Sf.Dbdeploy
{
    public class ToPrintStreamDeployer
    {
        private DirectoryInfo dir;

        private TextWriter doOutputPrintStream;
        private TextWriter undoOutputPrintStream;
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

        public void doDeploy(int lastChangeToApply)
        {
            Console.Out.WriteLine("dbdeploy v2.11");

            ChangeScriptRepository repository =
                new ChangeScriptRepository(new DirectoryScanner().getChangeScriptsForDirectory(dir));
            ChangeScriptExecuter doScriptExecuter = new ChangeScriptExecuter(doOutputPrintStream, dbmsSyntax);
            Controller doController = new Controller(schemaManager, repository, doScriptExecuter);
            doController.ProcessDoChangeScripts(lastChangeToApply);
            doOutputPrintStream.Flush();
            if (undoOutputPrintStream != null)
            {
                ChangeScriptExecuter undoScriptExecuter = new ChangeScriptExecuter(undoOutputPrintStream, dbmsSyntax);
                Controller undoController = new Controller(schemaManager, repository, undoScriptExecuter);
                undoController.ProcessUndoChangeScripts(lastChangeToApply);
                undoOutputPrintStream.Flush();
            }
        }
    }
}