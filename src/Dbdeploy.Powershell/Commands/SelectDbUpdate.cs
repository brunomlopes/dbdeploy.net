using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using Net.Sf.Dbdeploy.Scripts;

namespace Dbdeploy.Powershell
{
}

namespace Dbdeploy.Powershell.Commands
{
    [Cmdlet(VerbsCommon.Select, "DbUpdate")]
    public class SelectDbUpdate : DbUpdateBase
    {
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            this._databaseSchemaVersion.GetAppliedChangeNumbers();
            var infoTextWriter = new LambdaTextWriter(WriteVerbose);
            List<ChangeScript> allChangeScripts = new DirectoryScanner(infoTextWriter).GetChangeScriptsForDirectory(new DirectoryInfo(_deltasDirectory));
            var repository = new ChangeScriptRepository(allChangeScripts);

            var changeScripts = repository.GetOrderedListOfDoChangeScripts();

            var descriptionPrettyPrinter = new DescriptionPrettyPrinter();

            var objects =
                changeScripts
                    .Select(script => new
                                          {
                                              Id = script.GetId(),
                                              Description = descriptionPrettyPrinter.Format(script.GetDescription()),
                                              File = script.GetFile()
                                          });

            WriteObject(objects, true);
        }
    }
}
