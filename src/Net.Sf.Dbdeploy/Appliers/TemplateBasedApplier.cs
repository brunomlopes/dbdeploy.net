using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Net.Sf.Dbdeploy.Database;
using Net.Sf.Dbdeploy.Scripts;
using NVelocity;
using NVelocity.App;

namespace Net.Sf.Dbdeploy.Appliers
{
    public class TemplateBasedApplier : IChangeScriptApplier
    {
        private readonly TextWriter writer;

        private readonly string syntax;

        private readonly string changeLogTableName;

        private readonly string delimiter;

        private readonly IDelimiterType delimiterType;

        private readonly DirectoryInfo templateDirectory;

        public TemplateBasedApplier(
            TextWriter writer,
            string syntax,
            string changeLogTableName,
            string delimiter,
            IDelimiterType delimiterType,
            DirectoryInfo templateDirectory)
        {
            this.templateDirectory = templateDirectory;
            this.delimiterType = delimiterType;
            this.delimiter = delimiter;
            this.changeLogTableName = changeLogTableName;
            this.syntax = syntax;
            this.writer = writer;
        }

        public void  Apply(IEnumerable<ChangeScript> changeScripts)
        {
            string filename = string.Format("{0}_{1}.vm", this.syntax, this.GetTemplateQualifier());

            var model = new Hashtable();
            
            model.Add("scripts", changeScripts);
            model.Add("changeLogTableName", this.changeLogTableName);
            model.Add("delimiter", this.delimiter);
            model.Add("seperator", this.delimiterType is RowDelimiter ? Environment.NewLine : string.Empty);

            var templateEngine = new VelocityEngine();
            var context = new VelocityContext(model);

            Template template = templateEngine.GetTemplate(
                Path.Combine(this.templateDirectory.FullName, filename));

            template.Merge(context, this.writer);
        }

        protected virtual string GetTemplateQualifier()
        {
            return "apply";
        }
    }
}
