using System.IO;
using System.Management.Automation;
using Net.Sf.Dbdeploy;

namespace Dbdeploy.Powershell.Commands
{
    [Cmdlet(VerbsData.Export, "DbUpdate")]
    public class ExportDbUpdate : DbUpdateBase
    {
        [Parameter]
        public string OutputFile { get; set; }

        [Parameter]
        public string UndoOutputFile { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (string.IsNullOrEmpty(this.OutputFile))
            {
                this.WriteError(new ErrorRecord(new PSInvalidOperationException(
                    "Missing a file for output"),
                    "NoOutputFile",
                    ErrorCategory.MetadataError,
                    null));

                return;
            }

            var dbDeploy = new DbDeployer
            {
                InfoWriter = new LambdaTextWriter(WriteVerbose),
                Dbms = this.DatabaseType,
                ConnectionString = this.ConnectionString,
                ChangeLogTableName = this.TableName,
                ScriptDirectory = new DirectoryInfo(this.deltasDirectory),
                AutoCreateChangeLogTable = this.AutoCreateChangeLogTable,
                UseSqlCmd = this.UseSqlCmd
            };

            dbDeploy.OutputFile = new FileInfo(this.ToAbsolutePath(OutputFile));

            if (!string.IsNullOrEmpty(this.UndoOutputFile))
            {
                dbDeploy.OutputFile = new FileInfo(this.ToAbsolutePath(UndoOutputFile));
            }

            dbDeploy.Go();
        }
    }
}