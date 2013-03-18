namespace Net.Sf.Dbdeploy.Database
{
    public class MsSqlDbmsSyntax : DbmsSyntax
    {
        public MsSqlDbmsSyntax()
            : base("mssql")
        {
        }

        public override string GenerateTimestamp()
        {
            return "getdate()";
        }

        public override string GenerateUser()
        {
            return "suser_name()";
        }
    }
}