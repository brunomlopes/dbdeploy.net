namespace Net.Sf.Dbdeploy.Database
{
    public class MySqlDbmsSyntax : DbmsSyntax
    {
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