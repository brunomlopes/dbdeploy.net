using System;

namespace Net.Sf.Dbdeploy.Exceptions
{
    public class SchemaVersionTrackingException : DbDeployException
    {
        public SchemaVersionTrackingException(string message, Exception inner) 
            : base(message, inner)
        {
        }

        public SchemaVersionTrackingException(string message) 
            : base(message)
        {
        }

        public SchemaVersionTrackingException(Exception inner)
            : base(inner)
        {
        }
    }
}