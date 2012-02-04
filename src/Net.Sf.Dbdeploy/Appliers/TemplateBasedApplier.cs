using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Net.Sf.Dbdeploy.Database;
using Net.Sf.Dbdeploy.Scripts;
using NVelocity;
using NVelocity.App;
using Net.Sf.Dbdeploy.Exceptions;
using NVelocity.Exception;
using Commons.Collections;

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
            if (writer == null)
                throw new ArgumentNullException("writer");

            if (syntax == null)
                throw new ArgumentNullException("syntax");

            this.writer = writer;
            this.syntax = syntax;
            this.delimiter = delimiter;
            this.changeLogTableName = changeLogTableName;

            this.delimiterType = delimiterType ?? new NormalDelimiter();
            
            this.templateDirectory = templateDirectory ?? new DirectoryInfo(@".\Resources");
        }

        public void  Apply(IEnumerable<ChangeScript> changeScripts)
        {
            string filename = string.Format("{0}_{1}.vm", this.syntax, this.GetTemplateQualifier());

            var model = new Hashtable();
            
            model.Add("scripts", changeScripts);
            model.Add("changeLogTableName", this.changeLogTableName);
            model.Add("delimiter", this.delimiter);
            model.Add("separator", this.delimiterType is RowDelimiter ? Environment.NewLine : string.Empty);

            try
            {
                ExtendedProperties props = new ExtendedProperties();
                props.SetProperty("file.resource.loader.path", this.templateDirectory.FullName);

                var templateEngine = new VelocityEngine(props);

                var context = new VelocityContext(model);

                Template template = templateEngine.GetTemplate(filename);

                template.Merge(context, this.writer);
            }
            catch (ResourceNotFoundException ex)
            {
                throw new UsageException(
                    "Could not find template named " + filename + Environment.NewLine 
                    + "Check that you have got the name of the database syntax correct.", 
                    ex);
            }
        }

        protected virtual string GetTemplateQualifier()
        {
            return "apply";
        }
    }
}
