namespace Net.Sf.Dbdeploy.Exceptions
{
    public class DbmsNotSupportedException : DbDeployException
    {
        public DbmsNotSupportedException(string message) : base(message)
        {
        }
    }
}