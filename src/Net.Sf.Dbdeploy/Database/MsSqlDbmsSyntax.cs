using System;

namespace Net.Sf.Dbdeploy.Database
{
    public class MsSqlDbmsSyntax : IDbmsSyntax
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
            return string.Empty;
        }

    	public string GenerateBeginTransaction()
    	{
    		return "BEGIN TRANSACTION" + Environment.NewLine;
    	}

    	public string GenerateCommitTransaction()
    	{
    		return Environment.NewLine + "COMMIT TRANSACTION";
    	}
    }
}