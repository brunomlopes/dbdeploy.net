namespace Net.Sf.Dbdeploy.Appliers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    using Commons.Collections;

    using NVelocityReplacements;
    using Database;
    using Exceptions;
    using Scripts;

    using NVelocity;
    using NVelocity.App;
    using NVelocity.Exception;

    public class TemplateBasedApplier : IChangeScriptApplier
    {
        private readonly TextWriter writer;

        private readonly IDbmsSyntax syntax;

        private readonly string changeLogTableName;

        private readonly string delimiter;

        private readonly IDelimiterType delimiterType;

        private readonly DirectoryInfo templateDirectory;

        public TemplateBasedApplier(
            TextWriter writer,
            IDbmsSyntax syntax,
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
            
            this.templateDirectory = templateDirectory;
        }

        public void Apply(IEnumerable<ChangeScript> changeScripts, bool createChangeLogTable)
        {
            string filename = syntax.GetTemplateFileNameFor(GetTemplateQualifier());

            var model = new Hashtable();
            
            model.Add("scripts", changeScripts);
            model.Add("changeLogTableName", changeLogTableName);
            model.Add("delimiter", delimiter);
            model.Add("separator", delimiterType is RowDelimiter ? Environment.NewLine : string.Empty);

            try
            {
                var props = new ExtendedProperties();

                var assemblyName = GetType().Assembly.GetName().Name;

                ReplaceManagersWithDbDeployVersions(props, assemblyName);

                if (templateDirectory == null)
                {
                    props.AddProperty("resource.loader", "assembly");
                    props.AddProperty("assembly.resource.loader.class",
                                      // See the ; there? It will be replaced by , in the resource loader factory
                                      // this is because if we add a property with a comma in the value, it will add *two* values to the property.
                                      // oh joy.
                                      typeof (DbDeployAssemblyResourceLoader).FullName + "; " + assemblyName);
                    props.AddProperty("assembly.resource.loader.assembly", assemblyName);
                    filename = "Net.Sf.Dbdeploy.Resources." + filename;
                }
                else
                {
                    props.SetProperty("file.resource.loader.path", templateDirectory.FullName);
                }

                if (createChangeLogTable)
                {
                    writer.Write(syntax.CreateChangeLogTableSqlScript(changeLogTableName));
                }

                var templateEngine = new VelocityEngine(props);

                var context = new VelocityContext(model);

                var template = templateEngine.GetTemplate(filename);

                template.Merge(context, writer);
            }
            catch (ResourceNotFoundException ex)
            {
                string locationMessage;
                if (templateDirectory == null)
                {
                    locationMessage = "";
                }
                else
                {
                    locationMessage = " at " + templateDirectory.FullName;
                }
                throw new UsageException(
                    "Could not find template named " + filename + locationMessage + Environment.NewLine 
                    + "Check that you have got the name of the database syntax correct.", 
                    ex);
            }
        }

        private static void ReplaceManagersWithDbDeployVersions(ExtendedProperties props, string assemblyName)
        {
            // our versions are straight subclasses of NVelocity's vanilla managers
            // EXCEPT ours will always be public, even when we ilmerge the assemblies
            var addStringProperty = props.GetType().GetMethod("AddStringProperty", BindingFlags.NonPublic | BindingFlags.Instance);

            addStringProperty.Invoke(props, new object[]
                {
                    "resource.manager.class",
                    typeof (DbDeployResourceManager).FullName + "," + assemblyName
                });

            addStringProperty.Invoke(props, new object[]
                {
                    "directive.manager",
                    typeof (DbDeployDirectiveManager).FullName + "," + assemblyName
                });
        }

        protected virtual string GetTemplateQualifier()
        {
            return "apply";
        }
    }
}
