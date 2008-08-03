namespace Net.Sf.Dbdeploy.Database
{
    public interface IDbmsSyntax
    {
        string GenerateScriptHeader();

        string GenerateTimestamp();

        string GenerateUser();

        string GenerateStatementDelimiter();

        string GenerateCommit();

    	string GenerateBeginTransaction();
    	
		string GenerateCommitTransaction();
    }
}