using System;

namespace Net.Sf.Dbdeploy.Exceptions
{
    public class UnrecognisedFilenameException : DbDeployException
    {
        public UnrecognisedFilenameException(String message, Exception cause) : base(message, cause)
        {
        }

        public UnrecognisedFilenameException(String message) : base(message)
        {
        }

        public UnrecognisedFilenameException(Exception cause)
            : base(cause)
        {
        }
    }
}