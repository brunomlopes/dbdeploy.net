namespace Net.Sf.Dbdeploy.Database
{
    public interface DbmsSyntax
    {
        string GenerateScriptHeader();

        string GenerateTimestamp();

        string GenerateUser();

        string GenerateStatementDelimiter();

        string GenerateCommit();
    }
}