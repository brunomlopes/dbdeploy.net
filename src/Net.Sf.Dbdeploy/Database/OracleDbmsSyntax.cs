namespace Net.Sf.Dbdeploy.Database
{
    public class OracleDbmsSyntax : DbmsSyntax
    {
        public OracleDbmsSyntax()
            : base("ora")
        {
        }

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