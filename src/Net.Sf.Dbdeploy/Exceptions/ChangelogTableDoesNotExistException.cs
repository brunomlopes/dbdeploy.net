using System;

namespace Net.Sf.Dbdeploy.Exceptions
{
    public class ChangelogTableDoesNotExistException : DbDeployException
    {
        public ChangelogTableDoesNotExistException(string message, Exception inner) : base(message, inner)
        {
        }

        public ChangelogTableDoesNotExistException(Exception inner) : base(inner)
        {
        }

        public ChangelogTableDoesNotExistException(string message) : base(message)
        {
        }
    }
}