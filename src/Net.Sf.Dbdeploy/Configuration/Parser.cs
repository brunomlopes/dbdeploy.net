using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Schema;

namespace Net.Sf.Dbdeploy.Configuration
{
    using Net.Sf.Dbdeploy.Database;

    /// <summary>
    /// Parser for command line arguments.
    /// </summary>
    public static class Parser
    {
        /// <summary>
        /// Parses the type of the delimiter.
        /// </summary>
        /// <param name="value">The string to parse.</param>
        /// <returns>The parsed value or default.</returns>
        public static IDelimiterType ParseDelimiterType(string value)
        {
            switch ((value ?? string.Empty).ToUpperInvariant())
            {
                case "ROW":
                    return new RowDelimiter();

                case "NORMAL":
                default:
                    return new NormalDelimiter();
            }
        }

        /// <summary>
        /// Parses the line ending.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <returns>The parsed value.</returns>
        public static string ParseLineEnding(string value)
        {
            switch ((value ?? string.Empty).ToUpperInvariant())
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

        public static IEnumerable<Assembly> ParseAssembly(string value)
        {
            var listaType = new List<Assembly>();
            if (!string.IsNullOrWhiteSpace(value))
            {
                var listaAssemblyInformado = value.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);

                foreach (var assembly in listaAssemblyInformado)
                {
                    var assemblyName = AssemblyName.GetAssemblyName(assembly);
                    listaType.Add(Assembly.Load(assemblyName));
                }
            }

            return listaType;
        }

        public static IEnumerable<DirectoryInfo> ParseDirectory(string rootPath, string value)
        {
            var listaType = new List<DirectoryInfo>();
            if (!string.IsNullOrWhiteSpace(value))
            {
                var listaDirectoryInformado = value.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);

                foreach (var directory in listaDirectoryInformado)
                {
                    var parsedDirectory = Path.IsPathRooted(directory) ? directory : Path.GetFullPath(Path.Combine(rootPath, directory));
                    if (Directory.Exists(parsedDirectory))
                    {
                        var dirInfo = new DirectoryInfo(parsedDirectory);
                        listaType.Add(dirInfo);
                    }
                }
            }

            return listaType;
        }

        public static IEnumerable<DirectoryInfo> ParseDirectory(string value)
        {
            var listaType = new List<DirectoryInfo>();
            if (!string.IsNullOrWhiteSpace(value))
            {
                var listaDirectoryInformado = value.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);

                foreach (var directory in listaDirectoryInformado)
                {
                    if (Directory.Exists(directory))
                    {
                        var dirInfo = new DirectoryInfo(directory);
                        listaType.Add(dirInfo);
                    }
                }
            }

            return listaType;
        }

        public static Func<string, bool> ParseAssemblyFilterByName(string value)
        {
            return resourceName => resourceName.Contains(value);
        }

        public static bool ParseAssemblyOnly(string value)
        {
            return value.ToUpper() == bool.TrueString.ToUpper();
        }
    }
}