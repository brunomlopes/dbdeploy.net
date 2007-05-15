using System;

namespace Net.Sf.Dbdeploy.Database
{
    public class MsSqlDbmsSyntax : DbmsSyntax
    {
        public string GenerateScriptHeader()
        {
            return string.Empty;
        }

        public string GenerateTimestamp()
        {
            return "getdate()";
        }

        public string GenerateUser()
        {
            return "user_name()";
        }

        public string GenerateStatementDelimiter()
        {
            return "\nGO";
        }

        public string GenerateCommit()
        {
            return string.Empty;
        }
    }
}