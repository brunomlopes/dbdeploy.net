using System;

namespace Net.Sf.Dbdeploy.Exceptions
{
    public class RequiredChangeScriptNotFoundException : DbDeployException
    {
        public RequiredChangeScriptNotFoundException(String message, Exception cause) : base(message, cause)
        {
        }

        public RequiredChangeScriptNotFoundException(String message) : base(message)
        {
        }

        public RequiredChangeScriptNotFoundException(Exception cause)
            : base(cause)
        {
        }
    }
}