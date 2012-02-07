using System;

namespace Net.Sf.Dbdeploy.Database
{
    /// <summary>
    /// Delimiter is interpreted whenever it appears at the end of a line
    /// </summary>
    public class NormalDelimiter : IDelimiterType
    {
        public bool Matches(string line, string delimiter)
        {
            return line != null && line.EndsWith(delimiter, StringComparison.OrdinalIgnoreCase);
        }
    }
}
