using System;

namespace Net.Sf.Dbdeploy.Exceptions
{
    public class PriorFailedScriptException : DbDeployException
    {
        public PriorFailedScriptException(string message, Exception inner) 
            : base(message, inner)
        {
        }

        public PriorFailedScriptException(string message) 
            : base(message)
        {
        }

        public PriorFailedScriptException(Exception inner) 
            : base(inner)
        {
        }
    }
}