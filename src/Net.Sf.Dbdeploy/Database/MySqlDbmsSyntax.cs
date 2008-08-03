using System;

namespace Net.Sf.Dbdeploy.Database
{
    public class MySqlDbmsSyntax : IDbmsSyntax
    {
        public string GenerateScriptHeader()
        {
            return String.Empty;
        }

        public string GenerateTimestamp()
        {
            return "CURRENT_TIMESTAMP";
        }

        public string GenerateUser()
        {
            return "USER()";
        }

        public string GenerateStatementDelimiter()
        {
            return ";";
        }

        public string GenerateCommit()
        {
            return "COMMIT" + GenerateStatementDelimiter();
        }

    	public string GenerateBeginTransaction()
    	{
			return "START TRANSACTION" + GenerateStatementDelimiter() + Environment.NewLine;
    	}

    	public string GenerateCommitTransaction()
    	{
    		return Environment.NewLine + GenerateCommit();
    	}
    }
}
    