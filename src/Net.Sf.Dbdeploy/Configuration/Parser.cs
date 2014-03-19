using System;
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

        public static Assembly ParseAssembly(string value)
        {
            var assemblyName = AssemblyName.GetAssemblyName(value);
            return Assembly.Load(assemblyName);
        }

        public static Func<string, bool> ParseAssemblyFilterByName(string value)
        {
            return resourceName => resourceName.Contains(value);
        }
    }
}