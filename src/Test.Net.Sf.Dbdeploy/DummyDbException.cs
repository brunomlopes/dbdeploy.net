using System.Data.Common;

namespace Net.Sf.Dbdeploy
{
    public class DummyDbException : DbException
    {
        public DummyDbException()
            : base("dummy exception")
        {
        }
    }
}
