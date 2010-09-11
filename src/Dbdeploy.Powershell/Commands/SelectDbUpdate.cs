using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Text;
using Net.Sf.Dbdeploy;
using Net.Sf.Dbdeploy.Configuration;
using Net.Sf.Dbdeploy.Database;
using Net.Sf.Dbdeploy.Scripts;

namespace Dbdeploy.Powershell.Commands
{
    [Cmdlet(VerbsCommon.Select, "DbUpdate")]
    public class SelectDbUpdate : PSCmdlet
    {
        public SelectDbUpdate()
        {
            DeltasDirectory = ".";
        }

        protected override void ProcessRecord()
        {
            var configurationFile = ToAbsolutePath(ConfigurationFile);
            var deltasDirectory = ToAbsolutePath(DeltasDirectory);


            IConfiguration config = new XmlConfiguration(configurationFile);
            DbmsFactory factory = new DbmsFactory(config.DbType, config.DbConnectionString);
            DatabaseSchemaVersionManager databaseSchemaVersion = new DatabaseSchemaVersionManager(factory,
                                                                                                  config.DbDeltaSet,
                                                                                                  config.CurrentDbVersion,
                                                                                                  config.TableName);


            if (!string.IsNullOrEmpty(UndoOutputFile) && string.IsNullOrEmpty(OutputFile))
            {
                WriteError(new ErrorRecord(new PSInvalidOperationException("Missing a file for output (just picked one for undo output)"),
                                           "NoUndoOutputFile", ErrorCategory.MetadataError, null));
                return;
            }

            var infoTextWriter = new LambdaTextWriter(WriteVerbose);

            TextWriter outputTextWriter;
            if (string.IsNullOrEmpty(OutputFile))
            {
                outputTextWriter = new LambdaTextWriter(WriteObject);
            }
            else
            {
                var outputFile = ToAbsolutePath(OutputFile);
                WriteObject(string.Format("Writing update script to {0}", outputFile));
                outputTextWriter = new StreamWriter(File.OpenWrite(outputFile), Encoding.UTF8);
                infoTextWriter = new LambdaTextWriter(WriteObject);
            }

            TextWriter undoTextWriter = null;
            if (!string.IsNullOrEmpty(UndoOutputFile))
            {
                var undoOutputFile = ToAbsolutePath(UndoOutputFile);
                WriteObject(string.Format("Writing undo update script to {0}", undoOutputFile));
                undoTextWriter = new StreamWriter(File.OpenWrite(undoOutputFile), Encoding.UTF8);
            }

            infoTextWriter.WriteLine("dbdeploy v2.12");

            new PowershellPrintStreamDeployer(databaseSchemaVersion, new DirectoryInfo(deltasDirectory),
                                              outputTextWriter,
                                              factory.CreateDbmsSyntax(), config.UseTransaction, undoTextWriter,
                                              infoTextWriter)
                .DoDeploy(Int32.MaxValue, infoTextWriter);
        }

        private string ToAbsolutePath(string deltasDirectory)
        {
            if (!Path.IsPathRooted(deltasDirectory))
            {
                deltasDirectory = Path.Combine(SessionState.Path.CurrentFileSystemLocation.Path, deltasDirectory);
            }
            return deltasDirectory;
        }

        [Parameter(Mandatory = true, Position = 0)]
        public string ConfigurationFile { get; set; }

        [Parameter(Position = 1)]
        public string DeltasDirectory { get; set; }

        [Parameter]
        public string OutputFile { get; set; }

        [Parameter]
        public string UndoOutputFile { get; set; }
    }


    public class PowershellPrintStreamDeployer
    {
        private readonly DirectoryInfo dir;
        private readonly TextWriter doOutputPrintStream;
        private readonly TextWriter undoOutputPrintStream;
        private readonly TextWriter infoPrintStream;
        private readonly DatabaseSchemaVersionManager schemaManager;
        private readonly IDbmsSyntax dbmsSyntax;
        private readonly bool useTransaction;

        public PowershellPrintStreamDeployer(DatabaseSchemaVersionManager schemaManager, DirectoryInfo dir,
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
        }

        public void DoDeploy(int lastChangeToApply, TextWriter infoTextWriter)
        {
            List<ChangeScript> changeScripts = new DirectoryScanner(infoTextWriter).GetChangeScriptsForDirectory(dir);
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
