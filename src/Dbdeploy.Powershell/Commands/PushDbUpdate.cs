using System.IO;
using System.Management.Automation;
using Net.Sf.Dbdeploy;

namespace Dbdeploy.Powershell.Commands
{
    [Cmdlet(VerbsCommon.Push, "DbUpdate")]
    public class PushDbUpdate: DbUpdateBase
    {
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var dbDeploy = new DbDeployer
            {
                InfoWriter = new LambdaTextWriter(WriteVerbose),
                Dbms = DatabaseType,
                ConnectionString = ConnectionString,
                ChangeLogTableName = TableName,
                ScriptDirectory = new DirectoryInfo(deltasDirectory),
            };

            dbDeploy.Go();
        }
    }
}