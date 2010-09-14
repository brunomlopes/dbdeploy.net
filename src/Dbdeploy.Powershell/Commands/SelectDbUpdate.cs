using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Text.RegularExpressions;
using Net.Sf.Dbdeploy.Scripts;

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


            var objects =
                changeScripts.Select(
                    script => new {Id = script.GetId(), Description = script.GetDescription(), File = script.GetFile()});

            WriteObject(objects, true);
        }
    }

    public class DescriptionPrettyPrinter
    {
        public string Format(string description)
        {
            var newDescription= new StringBuilder();

            var numberMatch = Regex.Match(description, "$[0-9]+(_- )");
            new Regex().
            numberMatch.
        }
    }
}
