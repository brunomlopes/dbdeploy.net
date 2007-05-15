using System.Configuration;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Database
{
    [TestFixture]
    public class DbmsFactoryTest
    {
        private static readonly string CONNECTION_STRING = ConfigurationManager.AppSettings["ConnString"];

        [Test]
        public void TestCanLoadMsSqlProvider()
        {
            DbmsFactory factory = new DbmsFactory("mssql", CONNECTION_STRING);
            DatabaseProvider provider = factory.Providers.GetProvider("mssql");
            Assert.IsNotNull(provider);
        }
    }
}