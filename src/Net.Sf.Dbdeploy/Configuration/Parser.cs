using System;
using System.Collections.Generic;
using System.Reflection;

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

        public static IEnumerable<Type> ParseType(string value)
        {
            var listaAssemblyInformado = value.Split(';');
            var listaType = new List<Type>();
            foreach (var assembly in listaAssemblyInformado)
            {
                var assemblyName = AssemblyName.GetAssemblyName(assembly);
                listaType.Add(assemblyName.GetType());
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