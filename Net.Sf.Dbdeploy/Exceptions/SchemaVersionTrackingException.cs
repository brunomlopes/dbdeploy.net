using System;

namespace Net.Sf.Dbdeploy.Exceptions
{
    public class SchemaVersionTrackingException : DbDeployException
    {
        public SchemaVersionTrackingException(String message, Exception cause) : base(message, cause)
        {
        }

        public SchemaVersionTrackingException(String message) : base(message)
        {
        }

        public SchemaVersionTrackingException(Exception cause)
            : base(cause)
        {
        }
    }
}