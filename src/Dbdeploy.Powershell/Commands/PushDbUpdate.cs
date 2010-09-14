using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Management.Automation;
using System.Text;
using Net.Sf.Dbdeploy.Scripts;

namespace Dbdeploy.Powershell.Commands
{
    [Cmdlet(VerbsCommon.Push, "DbUpdate")]
    public class PushDbUpdate: DbUpdateBase
    {
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var infoTextWriter = new LambdaTextWriter(WriteVerbose);
                
            var updateText = new StringBuilder();
            var stringWriterForUpdateScript = new StringWriter(updateText);

            List<ChangeScript> changeScripts = new DirectoryScanner(infoTextWriter).GetChangeScriptsForDirectory(new DirectoryInfo(_deltasDirectory));
            new PowershellPrintStreamDeployer(_databaseSchemaVersion, new ChangeScriptRepository(changeScripts),
                                              stringWriterForUpdateScript,
                                              _dbmsFactory.CreateDbmsSyntax(), _config.UseTransaction, null,
                                              infoTextWriter)
                .DoDeploy(Int32.MaxValue, infoTextWriter);

            using (var connection = _dbmsFactory.CreateConnection())
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = updateText.ToString();
                var result = command.ExecuteNonQuery();
                WriteObject(new {RowsChanged = result});
            }
        }

        protected override bool ForDirectExecution
        {
            get { return true; }
        }
    }
}