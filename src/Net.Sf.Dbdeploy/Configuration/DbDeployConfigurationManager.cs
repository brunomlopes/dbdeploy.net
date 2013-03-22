namespace Net.Sf.Dbdeploy.Configuration
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    /// <summary>
    /// Manager for handling DbDeploy configuration files.
    /// </summary>
    public class DbDeployConfigurationManager : IDbDeployConfigurationManager
    {
        /// <summary>
        /// Reads the configuration.
        /// </summary>
        /// <param name="xmlFilePath">The XML file path.</param>
        /// <returns>Configuration set.</returns>
        public DbDeploymentsConfig ReadConfiguration(string xmlFilePath)
        {
            DbDeploymentsConfig config;
            using (var reader = new StreamReader(File.OpenRead(xmlFilePath)))
            {
                config = this.ReadConfiguration(reader);
            }

            return config;
        }

        /// <summary>
        /// Reads the configuration.
        /// </summary>
        /// <param name="xmlFile">The XML file text reader.</param>
        /// <returns>Configuration set.</returns>
        public DbDeploymentsConfig ReadConfiguration(TextReader xmlFile)
        {
            var doc = XDocument.Load(xmlFile);

            var deploymentsConfig = new DbDeploymentsConfig();
            deploymentsConfig.Deployments = doc.Root.Descendants("dbdeploy").Select(this.ReadConfigElement).ToList();
            return deploymentsConfig;
        }

        /// <summary>
        /// Reads the config element into a configuration.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>Configuration read.</returns>
        public DbDeployConfig ReadConfigElement(XElement element)
        {
            var config = new DbDeployConfig();
            config.Dbms = GetAttribute(element, "dbms", DbDeployDefaults.Dbms);
            config.ConnectionString = GetAttribute(element, "connectionString", DbDeployDefaults.ConnectionString);
            config.ScriptDirectory = GetAttribute(element, "scriptDirectory", DbDeployDefaults.ScriptDirectory, v => new DirectoryInfo(v));
            config.OutputFile = GetAttribute(element, "outputFile", DbDeployDefaults.OutputFile, v => new FileInfo(v));
            config.ChangeLogTableName = GetAttribute(element, "changeLogTableName", DbDeployDefaults.ChangeLogTableName);
            config.AutoCreateChangeLogTable = GetAttribute(element, "autoCreateChangeLogTable", DbDeployDefaults.AutoCreateChangeLogTable, bool.Parse);
            config.ForceUpdate = GetAttribute(element, "forceUpdate", DbDeployDefaults.ForceUpdate, bool.Parse);
            config.UseSqlCmd = GetAttribute(element, "useSqlCmd", DbDeployDefaults.UseSqlCmd, bool.Parse);
            config.LastChangeToApply = GetAttribute(element, "lastChangeToApply", DbDeployDefaults.LastChangeToApply, v => new UniqueChange(v));
            config.Encoding = GetAttribute(element, "encoding", DbDeployDefaults.Encoding, v => new OutputFileEncoding(v).AsEncoding());
            config.TemplateDirectory = GetAttribute(element, "templateDirectory", DbDeployDefaults.TemplateDirectory, v => new DirectoryInfo(v));
            config.Delimiter = GetAttribute(element, "delimiter", DbDeployDefaults.Delimiter);
            config.DelimiterType = GetAttribute(element, "delimiterType", DbDeployDefaults.DelimiterType, Parser.ParseDelimiterType);
            config.LineEnding = GetAttribute(element, "lineEnding", DbDeployDefaults.LineEnding, Parser.ParseLineEnding);

            return config;
        }

        /// <summary>
        /// Gets the attribute value.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>
        /// Type to retrieve.
        /// </returns>
        private static string GetAttribute(XElement element, string name, string defaultValue)
        {
            var attribute = element.Attribute(name);
            return attribute != null ? attribute.Value : defaultValue;
        }

        /// <summary>
        /// Gets the attribute value as the specified type.
        /// </summary>
        /// <typeparam name="T">Type of attribute.</typeparam>
        /// <param name="element">The element.</param>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value to return if the attribute is not present.</param>
        /// <param name="parser">The string parser for the specified type.</param>
        /// <returns>
        /// Type to retrieve.
        /// </returns>
        private static T GetAttribute<T>(XElement element, string name, T defaultValue, Func<string, T> parser)
        {
            var value = GetAttribute(element, name, null);
            return !string.IsNullOrWhiteSpace(value) ? parser(value) : defaultValue;
        }
    }
}