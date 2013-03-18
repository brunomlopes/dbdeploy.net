using System;
using System.Collections.Generic;
using System.IO;
using NDesk.Options;
using Net.Sf.Dbdeploy.Database;
using Net.Sf.Dbdeploy.Exceptions;

namespace Net.Sf.Dbdeploy
{
    public class Parser
    {
        public void Parse(string[] args, DbDeployer dbDeploy)
        {
            try
            {
                dbDeploy.ScriptDirectory = new DirectoryInfo(".");

                OptionSet options = this.Initialize(dbDeploy);

                List<string> unknown = options.Parse(args);

                if (unknown != null && unknown.Count != 0)
                {
                    foreach (var s in unknown)
                    {
                        // empty "unkown" parameters are allowed
                        if (s != null && !string.IsNullOrEmpty(s.Trim()))
                            throw new UsageException("Unkown parameter(s): " + string.Join(", ", unknown.ToArray()));
                    }
                }
            }
            catch (OptionException e)
            {
                throw new UsageException(e.Message, e);
            }
        }

        public void PrintUsage()
        {
            OptionSet options = this.Initialize(null);

            options.WriteOptionDescriptions(Console.Out);
        }

        private OptionSet Initialize(DbDeployer dbDeploy) 
        {
            OptionSet options = new OptionSet();


            options
                .Add(
                    "d|dbms=",
                    "DBMS type ('mssql', 'mysql' or 'ora')",
                    (string s) => dbDeploy.Dbms = s)

                .Add(
                    "c|connectionstring=",
                    "connection string for database",
                    (string s) => dbDeploy.ConnectionString = StripQuotes(s))

                .Add(
                    "s|scriptdirectory=",
                    "directory containing change scripts (default: .)",
                    (string s) => dbDeploy.ScriptDirectory = new DirectoryInfo(StripQuotes(s)))

                .Add(
                    "e|encoding=",
                    "encoding for input and output files (default: UTF-8)",
                    (string s) => dbDeploy.Encoding = new OutputFileEncoding(StripQuotes(s)).AsEncoding())

                .Add(
                    "o|outputfile=",
                    "output file",
                    (string s) => dbDeploy.OutputFile = new FileInfo(StripQuotes(s)))

                .Add(
                    "templatedir=",
                    "template directory",
                    (string s) => dbDeploy.TemplateDir = new DirectoryInfo(StripQuotes(s)))

                .Add(
                    "t|changelogtablename=",
                    "name of change log table to use (default: ChangeLog)",
                    (string s) => dbDeploy.ChangeLogTableName = StripQuotes(s))

                .Add(
                    "a|autocreatechangetable=",
                    "automatically creates the change log table if it does not exist (true or false).  Defaults to true.",
                    (string s) => dbDeploy.AutoCreateChangeLogTable = s.ToLowerInvariant() != "false")

                .Add(
                    "delimiter=",
                    "delimiter to separate sql statements",
                    (string s) => dbDeploy.Delimiter = s)

                .Add(
                    "delimitertype=",
                    "delimiter type to separate sql statements (row or normal)",
                    (string s) => dbDeploy.DelimiterType = ParseDelimiterType(s))

                .Add(
                    "lineending=",
                    "line ending to use when applying scripts direct to db (platform, cr, crlf, lf)",
                    (string s) => dbDeploy.LineEnding = ParseLineEnding(s));
            
            return options;
        }

        private static string StripQuotes(string s)
        {
            if (s != null)
            {
                s = s.Trim();

                if ((s.StartsWith("\"", StringComparison.OrdinalIgnoreCase) && s.EndsWith("\"", StringComparison.OrdinalIgnoreCase))
                    || (s.StartsWith("'", StringComparison.OrdinalIgnoreCase) && s.EndsWith("'", StringComparison.OrdinalIgnoreCase)))
                {
                    return s.Substring(1, s.Length - 2);
                }
            }

            return s;
        }

        private static IDelimiterType ParseDelimiterType(string s)
        {
            switch ((s ?? string.Empty).ToUpperInvariant())
            {
                case "ROW":
                    return new RowDelimiter();

                case "NORMAL":
                default:
                    return new NormalDelimiter();
            }
        }

        private static string ParseLineEnding(string s)
        {
            switch ((s ?? string.Empty).ToUpperInvariant())
            {
                case "CR":
                    return LineEnding.Cr;

                case "CRLF":
                    return LineEnding.CrLf;

                case "LF":
                    return LineEnding.Lf;

                default:
                case "PLATFORM":
                    return LineEnding.Platform;
            }
        }

    }
}