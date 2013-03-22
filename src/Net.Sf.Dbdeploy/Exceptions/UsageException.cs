namespace Net.Sf.Dbdeploy.Exceptions
{
    using System;

    public class UsageException : DbDeployException
    {
        public UsageException(string message) 
            : base(message) 
        {
        }
        
        public UsageException(string message, Exception inner) 
            : base(message, inner) 
        {
        }

        public static void ThrowForMissingRequiredValue(string valueName)
        {
            throw new UsageException(valueName + " required");
        }
    }
}