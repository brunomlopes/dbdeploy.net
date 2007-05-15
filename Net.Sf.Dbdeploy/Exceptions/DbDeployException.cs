using System;

namespace Net.Sf.Dbdeploy.Exceptions
{
    public class DbDeployException : Exception
    {
        public DbDeployException(String message)
            : base(message)
        {
        }

        public DbDeployException(String message, Exception cause)
            : base(message, cause)
        {
        }

        public DbDeployException(Exception cause)
            : base(cause.Message)
        {
        }
    }
}