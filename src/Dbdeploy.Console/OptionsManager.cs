namespace Net.Sf.Dbdeploy
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using NDesk.Options;

    using Net.Sf.Dbdeploy.Configuration;
    using Net.Sf.Dbdeploy.Exceptions;

    /// <summary>
    /// Manages all options for the command line.
    /// </summary>
    public static class OptionsManager
    {
        /// <summary>
        /// Prints the options usage.
        /// </summary>
        public static void PrintUsage()
        {
            OptionSet options = Initialize(null, new ConfigFileInfo());

            options.WriteOptionDescriptions(Console.Out);
        }

        /// <summary>
        /// Parses the specified args into a deployments configuration.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <returns>
        /// Configuration set.
        /// </returns>
        /// <exception cref="UsageException">Throws when unknown or invalid parameters are found.</exception>
        public static DbDeploymentsConfig ParseOptions(string[] args)
        {
            // Initialize configuration with a single deployment.
            var deploymentsConfig = new DbDeploymentsConfig();
            try
            {
                var configFile = new ConfigFileInfo();
                var config = new DbDeployConfig();
                OptionSet options = Initialize(config, configFile);
                deploymentsConfig.Deployments.Add(config);

                List<string> unknown = options.Parse(args);

                if (unknown != null && unknown.Count != 0)
                {
                    foreach (var s in unknown)
                    {
                        // empty "unkown" parameters are allowed
                        if (s != null && !string.IsNullOrEmpty(s.Trim()))
                        {
                            throw new UsageException("Unkown parameter(s): " + string.Join(", ", unknown.ToArray()));
                        }
                    }
                }

                // If a configuration file was specified in the command, use that instead of options.
                if (configFile.FileInfo != null)
                {
                    var configurationManager = new DbDeployConfigurationManager();
                    deploymentsConfig = configurationManager.ReadConfiguration(configFile.FileInfo.FullName);
                }
            }
            catch (OptionException e)
            {
                throw new UsageException(e.Message, e);
            }

            return deploymentsConfig;
        }

        /// <summary>
        /// Initializes the specified config.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="configFile">The config file to read if found.</param>
        /// <returns>
        /// Option set for the config.
        /// </returns>
        private static OptionSet Initialize(DbDeployConfig config, ConfigFileInfo configFile) 
        {
            var options = new OptionSet();

            options
                .Add(
                    "d|dbms=",
                    "DBMS type ('mssql', 'mysql', 'ora' or 'db2')",
                    s => config.Dbms = s)

                .Add(
                    "c|connectionstring=",
                    "connection string for database",
                    s => config.ConnectionString = StripQuotes(s))

                .Add(
                    "s|scriptdirectory=",
                    "directory containing change scripts (default: .)",
                    s => config.ScriptDirectory = new DirectoryInfo(StripQuotes(s)))

                .Add(
                    "o|outputfile=",
                    "output file",
                    s => config.OutputFile = new FileInfo(StripQuotes(s)))

                .Add(
                    "t|changelogtablename=",
                    "name of change log table to use (default: ChangeLog)",
                    s => config.ChangeLogTableName = StripQuotes(s))

                .Add(
                    "a|autocreatechangelogtable=",
                    "automatically creates the change log table if it does not exist (true or false).  Defaults to true.",
                    s => config.AutoCreateChangeLogTable = s.ToLowerInvariant() != "false")

                .Add(
                    "f|forceupdate=",
                    "forces previously failed scripts to be run again (true or false).  Defaults to false.",
                    s => config.ForceUpdate = s.ToLowerInvariant() == "true")

                .Add(
                    "u|usesqlcmd=",
                    "runs scripts in SQLCMD mode (true or false).  Defaults to false.",
                    s => config.UseSqlCmd = s.ToLowerInvariant() == "true")

                .Add(
                    "l|lastchangetoapply=",
                    "sets the last change to apply in the form of folder/scriptnumber (v1.0.0/4).",
                    s => config.LastChangeToApply = !string.IsNullOrWhiteSpace(s) ? new UniqueChange(s) : null)

                .Add(
                    "e|encoding=",
                    "encoding for input and output files (default: UTF-8)",
                    s => config.Encoding = new OutputFileEncoding(StripQuotes(s)).AsEncoding())

                .Add(
                    "templatedirectory=",
                    "template directory",
                    s => config.TemplateDirectory = new DirectoryInfo(StripQuotes(s)))

                .Add(
                    "delimiter=",
                    "delimiter to separate sql statements",
                    s => config.Delimiter = s)

                .Add(
                    "delimitertype=",
                    "delimiter type to separate sql statements (row or normal)",
                    s => config.DelimiterType = Parser.ParseDelimiterType(s))

                .Add(
                    "lineending=",
                    "line ending to use when applying scripts direct to db (platform, cr, crlf, lf)",
                    s => config.LineEnding = Parser.ParseLineEnding(s))

                .Add(
                    "config=",
                    "configuration file to use for all settings.",
                    s => configFile.FileInfo = !string.IsNullOrWhiteSpace(s) ? new FileInfo(StripQuotes(s)) : null);
            
            return options;
        }

        /// <summary>
        /// Strips the quotes from around the value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>Value with quotes removed.</returns>
        private static string StripQuotes(string value)
        {
            if (value != null)
            {
                value = value.Trim();

                if ((value.StartsWith("\"", StringComparison.OrdinalIgnoreCase) && value.EndsWith("\"", StringComparison.OrdinalIgnoreCase))
                    || (value.StartsWith("'", StringComparison.OrdinalIgnoreCase) && value.EndsWith("'", StringComparison.OrdinalIgnoreCase)))
                {
                    return value.Substring(1, value.Length - 2);
                }
            }

            return value;
        }
    }
}