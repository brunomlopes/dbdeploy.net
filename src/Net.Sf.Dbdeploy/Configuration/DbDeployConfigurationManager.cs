using Net.Sf.Dbdeploy.Database;

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
                config = ReadConfiguration(reader, Path.GetDirectoryName(xmlFilePath));
            }

            return config;
        }

        /// <summary>
        /// Reads the configuration.
        /// </summary>
        /// <param name="xmlFile">The XML file text reader.</param>
        /// <param name="rootPath">The root path to resolve relative paths to.</param>
        /// <returns>
        /// Configuration set.
        /// </returns>
        public DbDeploymentsConfig ReadConfiguration(TextReader xmlFile, string rootPath)
        {
            var doc = XDocument.Load(xmlFile);

            var deploymentsConfig = new DbDeploymentsConfig();
            deploymentsConfig.Deployments = doc.Root.Descendants("dbdeploy").Select(e => ReadConfigElement(e, rootPath)).ToList();
            return deploymentsConfig;
        }

        /// <summary>
        /// Reads the config element into a configuration.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="rootPath">The root path to resolve relative paths to.</param>
        /// <returns>
        /// Configuration read.
        /// </returns>
        public DbDeployConfig ReadConfigElement(XElement element, string rootPath)
        {
            var config = new DbDeployConfig();
            config.Dbms = GetAttribute(element, "dbms", DbDeployDefaults.Dbms);
            config.ConnectionString = GetAttribute(element, "connectionString", DbDeployDefaults.ConnectionString);
            config.ScriptDirectory = GetAttribute(element, "scriptDirectory", DbDeployDefaults.ScriptDirectory, v => new DirectoryInfo(ResolveRelativePath(rootPath, v)));
            config.OutputFile = GetAttribute(element, "outputFile", DbDeployDefaults.OutputFile, v => new FileInfo(ResolveRelativePath(rootPath, v)));
            config.ChangeLogTableName = GetAttribute(element, "changeLogTableName", DbDeployDefaults.ChangeLogTableName);
            config.AutoCreateChangeLogTable = GetAttribute(element, "autoCreateChangeLogTable", DbDeployDefaults.AutoCreateChangeLogTable, bool.Parse);
            config.ForceUpdate = GetAttribute(element, "forceUpdate", DbDeployDefaults.ForceUpdate, bool.Parse);
            config.UseSqlCmd = GetAttribute(element, "useSqlCmd", DbDeployDefaults.UseSqlCmd, bool.Parse);
            config.LastChangeToApply = GetAttribute(element, "lastChangeToApply", DbDeployDefaults.LastChangeToApply, v => new UniqueChange(v));
            config.Encoding = GetAttribute(element, "encoding", DbDeployDefaults.Encoding, v => new OutputFileEncoding(v).AsEncoding());
            config.TemplateDirectory = GetAttribute(element, "templateDirectory", DbDeployDefaults.TemplateDirectory, v => new DirectoryInfo(ResolveRelativePath(rootPath, v)));
            config.Delimiter = GetAttribute(element, "delimiter", DbDeployDefaults.Delimiter);
            config.DelimiterType = GetAttribute(element, "delimiterType", DbDeployDefaults.DelimiterType, Parser.ParseDelimiterType);
            config.LineEnding = GetAttribute(element, "lineEnding", DbDeployDefaults.LineEnding, Parser.ParseLineEnding);

            config.ScriptAssemblies = GetAttribute(element, "assembly", DbDeployDefaults.ScriptAssemblies, Parser.ParseAssembly);
            config.AssemblyResourceNameFilter = GetAttribute(element, "assemblyFilterByName", DbDeployDefaults.AssemblyResourceNameFilter, Parser.ParseAssemblyFilterByName);
            //config.AssemblyOnly = GetAttribute(element, "assemblyOnly", DbDeployDefaults.AssemblyOnly, Parser.ParseAssemblyOnly);

            return config;
        }

        /// <summary>
        /// Resolves the relative path to an absolute path relative to the specified root path.
        /// </summary>
        /// <param name="rootPath">The root path to resolve relative to.</param>
        /// <param name="path">The relative path to resolve.</param>
        /// <returns>Full path.</returns>
        private static string ResolveRelativePath(string rootPath, string path)
        {
            return Path.IsPathRooted(path) ? path : Path.GetFullPath(Path.Combine(rootPath, path));
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