namespace Net.Sf.Dbdeploy.Database
{
    public abstract class DbmsSyntax : IDbmsSyntax
	{
		public abstract string GenerateTimestamp();

		public abstract string GenerateUser();
	}
}