namespace Net.Sf.Dbdeploy.Database
{
    public class MySqlDbmsSyntax : DbmsSyntax
    {
        public MySqlDbmsSyntax()
            : base("mysql")
        {
        }

        public override string GenerateTimestamp()
        {
            return "CURRENT_TIMESTAMP";
        }

        public override string GenerateUser()
        {
            return "USER()";
        }
    }
}