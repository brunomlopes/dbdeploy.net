namespace Net.Sf.Dbdeploy.Database
{
    public interface IDbmsSyntax
    {
        string GenerateTimestamp();

        string GenerateUser();
    }
}