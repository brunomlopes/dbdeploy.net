using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Text;
using Net.Sf.Dbdeploy.Scripts;

namespace Dbdeploy.Powershell.Commands
{
    [Cmdlet(VerbsData.Export, "DbUpdate")]
    public class ExportDbUpdate : DbUpdateBase
    {
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (!string.IsNullOrEmpty(UndoOutputFile) && string.IsNullOrEmpty(OutputFile))
            {
                WriteError(new ErrorRecord(new PSInvalidOperationException("Missing a file for output (just picked one for undo output)"),
                                           "NoUndoOutputFile", ErrorCategory.MetadataError, null));
                return;
            }

            var infoTextWriter = new LambdaTextWriter(WriteVerbose);

            TextWriter outputTextWriter;
            var openedFiles = new List<FileStream>();

            try
            {
                if (string.IsNullOrEmpty(OutputFile))
                {
                    outputTextWriter = new LambdaTextWriter(WriteObject);
                }
                else
                {
                    var outputFile = ToAbsolutePath(OutputFile);
                    WriteObject(String.Format("Writing update script to {0}", outputFile));
                    var openedFile = File.OpenWrite(outputFile);
                    openedFiles.Add(openedFile);

                    outputTextWriter = new StreamWriter(openedFile, Encoding.UTF8);
                    infoTextWriter = new LambdaTextWriter(WriteObject);
                }

                TextWriter undoTextWriter = null;
                if (!string.IsNullOrEmpty(UndoOutputFile))
                {
                    var undoOutputFile = ToAbsolutePath(UndoOutputFile);
                    WriteObject(String.Format("Writing undo update script to {0}", undoOutputFile));
                    var openedFile = File.OpenWrite(undoOutputFile);
                    openedFiles.Add(openedFile);
                    undoTextWriter = new StreamWriter(openedFile, Encoding.UTF8);
                }

                infoTextWriter.WriteLine("dbdeploy v2.12");

                List<ChangeScript> changeScripts = new DirectoryScanner(infoTextWriter).GetChangeScriptsForDirectory(new DirectoryInfo(_deltasDirectory));
                new PowershellPrintStreamDeployer(_databaseSchemaVersion, new ChangeScriptRepository(changeScripts),
                                                  outputTextWriter,
                                                  _dbmsFactory.CreateDbmsSyntax(), UseTransaction, undoTextWriter,
                                                  infoTextWriter)
                    .DoDeploy(Int32.MaxValue, infoTextWriter);

            }
            finally
            {
                foreach(var file in openedFiles)
                {
                    file.Close();
                }
            }
        }

        [Parameter]
        public string OutputFile { get; set; }

        [Parameter]
        public string UndoOutputFile { get; set; }
    }
}