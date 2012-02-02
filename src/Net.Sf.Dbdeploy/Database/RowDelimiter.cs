namespace Net.Sf.Dbdeploy.Database
{
    /// <summary>
    /// Delimiter must be on a line all to itself
    /// </summary>
    public class RowDelimiter : IDelimiterType
    {
        public bool Matches(string line, string delimiter)
        {
            return line != null && line.Equals(delimiter);
        }
    }
}
