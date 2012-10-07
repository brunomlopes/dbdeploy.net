namespace Net.Sf.Dbdeploy.Database
{
    public class OracleDbmsSyntax : DbmsSyntax
    {
        public override string GenerateTimestamp()
        {
            return "CURRENT_TIMESTAMP";
        }

        public override string GenerateUser()
        {
            return "USER";
        }
    }
}