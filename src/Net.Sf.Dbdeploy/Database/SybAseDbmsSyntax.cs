using System;

namespace Net.Sf.Dbdeploy.Database
{
    public class SybAseDbmsSyntax : DbmsSyntax
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
            return Environment.NewLine + "GO";
        }

        public string GenerateCommit()
        {
            return "COMMIT" + GenerateStatementDelimiter();
        }
    }
}