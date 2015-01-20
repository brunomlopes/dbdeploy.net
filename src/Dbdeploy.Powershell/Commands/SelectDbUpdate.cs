using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using Net.Sf.Dbdeploy.Database;
using Net.Sf.Dbdeploy.Scripts;

namespace Dbdeploy.Powershell.Commands
{
    [Cmdlet(VerbsCommon.Select, "DbUpdate")]
    public class SelectDbUpdate : DbUpdateBase
    {
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var infoTextWriter = new LambdaTextWriter(WriteVerbose);

            List<ChangeScript> allChangeScripts = new DirectoryScanner(infoTextWriter, Encoding.UTF8)
                .GetChangeScriptsForDirectory(new DirectoryInfo(this.deltasDirectory));

            var repository = new ChangeScriptRepository(allChangeScripts);
            var changeScripts = repository.GetAvailableChangeScripts();

            DbmsFactory factory = new DbmsFactory(this.DatabaseType, this.ConnectionString);
            var queryExecuter = new QueryExecuter(factory);

            var schemaManager = new DatabaseSchemaVersionManager(queryExecuter, factory.CreateDbmsSyntax(), this.TableName);

            var appliedChanges = schemaManager.GetAppliedChanges()
                .ToDictionary(c => c.ScriptNumber);

            var descriptionPrettyPrinter = new DescriptionPrettyPrinter();

            var objects = changeScripts
                .Select(script => new
                {
                    Id = script.ScriptNumber,
                    Description = descriptionPrettyPrinter.Format(script.ScriptName),
                    File = script.FileInfo,
                    Status = appliedChanges.ContainsKey(script.ScriptNumber) ?
                                    (ScriptStatus?)appliedChanges[script.ScriptNumber].Status :
                                    null
                });

            this.WriteObject(objects, true);
        }
    }
}