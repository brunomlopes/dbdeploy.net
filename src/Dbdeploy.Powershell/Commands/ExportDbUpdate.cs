using System.IO;
using System.Management.Automation;
using Net.Sf.Dbdeploy;

namespace Dbdeploy.Powershell.Commands
{
    using Net.Sf.Dbdeploy.Configuration;

    [Cmdlet(VerbsData.Export, "DbUpdate")]
    public class ExportDbUpdate : DbUpdateBase
    {
        [Parameter(Mandatory = true)]
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

            var config = new DbDeployConfig
                             {
                                 Dbms = this.DatabaseType,
                                 ConnectionString = this.ConnectionString,
                                 ChangeLogTableName = this.TableName,
                                 ScriptDirectory = new DirectoryInfo(this.deltasDirectory),
                                 AutoCreateChangeLogTable = this.AutoCreateChangeLogTable,
                                 ForceUpdate = this.ForceUpdate,
                                 UseSqlCmd = this.UseSqlCmd,
                                 OutputFile = new FileInfo(this.ToAbsolutePath(this.OutputFile))
                             };

            if (!string.IsNullOrEmpty(this.UndoOutputFile))
            {
                config.OutputFile = new FileInfo(this.ToAbsolutePath(UndoOutputFile));
            }

            var deployer = new DbDeployer();
            deployer.Execute(config, new LambdaTextWriter(WriteVerbose));
        }
    }
}