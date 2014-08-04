using System;

namespace Net.Sf.Dbdeploy.Database
{
    public class SybaseDbmsSyntax : DbmsSyntax
    {
        public SybaseDbmsSyntax() : base("sybase")
        {
        }

        public override string CurrentTimestamp
        {
            get { throw new NotImplementedException(); }
        }

        public override string CurrentUser
        {
            get { throw new NotImplementedException(); }
        }
    }
}