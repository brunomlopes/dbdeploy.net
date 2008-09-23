using System;

namespace Net.Sf.Dbdeploy.Database
{
    public class MySqlDbmsSyntax : DbmsSyntax
    {
        public override string GenerateScriptHeader()
        {
            return String.Empty;
        }

        public override string GenerateTimestamp()
        {
            return "CURRENT_TIMESTAMP";
        }

        public override string GenerateUser()
        {
            return "USER()";
        }

        public override string GenerateStatementDelimiter()
        {
            return ";";
        }

        public override string GenerateCommit()
        {
            return "COMMIT" + GenerateStatementDelimiter();
        }

    	public override string GenerateBeginTransaction()
    	{
			return "START TRANSACTION" + GenerateStatementDelimiter() + Environment.NewLine;
    	}

    	public override string GenerateCommitTransaction()
    	{
    		return Environment.NewLine + GenerateCommit();
    	}
    }
}
    