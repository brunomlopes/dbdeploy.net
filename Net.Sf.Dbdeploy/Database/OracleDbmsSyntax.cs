namespace Net.Sf.Dbdeploy.Database
{
    public class OracleDbmsSyntax : DbmsSyntax
    {
        public string GenerateScriptHeader()
        {
            return string.Empty;
        }

        public string GenerateTimestamp()
        {
            return "CURRENT_TIMESTAMP";
        }

        public string GenerateUser()
        {
            return "USER";
        }

        public string GenerateStatementDelimiter()
        {
            return ";";
        }

        public string GenerateCommit()
        {
            return "COMMIT" + GenerateStatementDelimiter();
        }
    }
}