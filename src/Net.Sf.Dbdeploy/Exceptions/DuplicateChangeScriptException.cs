using System;

namespace Net.Sf.Dbdeploy.Exceptions
{
    public class DuplicateChangeScriptException : DbDeployException
    {
        public DuplicateChangeScriptException(String message, Exception cause) : base(message, cause)
        {
        }

        public DuplicateChangeScriptException(String message) : base(message)
        {
        }

        public DuplicateChangeScriptException(Exception cause) : base(cause)
        {
        }
    }
}