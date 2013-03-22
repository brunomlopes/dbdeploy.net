namespace Net.Sf.Dbdeploy
{
    using System.Data.Common;

    public class DummyDbException : DbException
    {
        public DummyDbException()
            : base("dummy exception")
        {
        }
    }
}
