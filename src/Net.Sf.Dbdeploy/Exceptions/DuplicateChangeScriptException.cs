using System;

namespace Net.Sf.Dbdeploy.Exceptions
{
    public class DuplicateChangeScriptException : DbDeployException
    {
        public DuplicateChangeScriptException(string message, Exception inner) 
            : base(message, inner)
        {
        }

        public DuplicateChangeScriptException(string message) 
            : base(message)
        {
        }

        public DuplicateChangeScriptException(Exception inner) 
            : base(inner)
        {
        }
    }
}