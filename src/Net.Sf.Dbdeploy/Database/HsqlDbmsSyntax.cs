namespace Net.Sf.Dbdeploy.Database
{
    public class HsqlDbmsSyntax : DbmsSyntax
    {
        public override string GenerateScriptHeader()
        {
            return string.Empty;
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
    		return string.Empty;
    	}

    	public override string GenerateCommitTransaction()
    	{
			return string.Empty;
		}
    }
}