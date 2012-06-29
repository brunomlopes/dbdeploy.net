namespace Net.Sf.Dbdeploy.Database
{
    public class MsSqlDbmsSyntax : DbmsSyntax
    {
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