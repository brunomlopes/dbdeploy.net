using System;
using System.Data;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Database
{
    class DbmsFactoryPostgreTest : DbmsFactoryAbstractTest
    {
        private const string PostgreConnectionString = "Server=localhost;Port=5432;Database=dbdeploy;User Id=postgres;Password=postgres;";
        private const string DbmsPostgre = "postgre";
        private const string PostgreDllName = "Npgsql.dll";
        private readonly string postGreDllPath = AppDomain.CurrentDomain.BaseDirectory + "\\Mocks\\Fixtures\\PostgreDllConnection\\" + PostgreDllName;

        [Test]
        [ExpectedException("System.IO.FileNotFoundException")]
        public override void GivenAnEntryWithoutDllConnectionShouldThrowsException()
        {
            var factory = new DbmsFactory(DbmsPostgre, PostgreConnectionString);
            var connection = OpenConnection(factory);
            Assert.AreEqual(ConnectionState.Open, connection.State);
        }

        [Test]
        public override void GivenAnEntryWithDllConnectionShouldOpenedSuccessful()
        {
            var factory = new DbmsFactory(DbmsPostgre, PostgreConnectionString, postGreDllPath);
            var connection = OpenConnection(factory);
            Assert.AreEqual(ConnectionState.Open, connection.State);
        }

        [Test]
        public override void ShouldExecuteSqlCommandSuccessful()
        {
            var factory = new DbmsFactory(DbmsPostgre, PostgreConnectionString, postGreDllPath);
            OpenConnection(factory);
            const string query = "SELECT CURRENT_DATE;";
            ExecuteNonQuery(query);
        }

        [Test]
        [ExpectedException("Npgsql.NpgsqlException")]
        public override void ShouldExecuteSqlCommandFailed()
        {
            var factory = new DbmsFactory(DbmsPostgre, PostgreConnectionString, postGreDllPath);
            OpenConnection(factory);
            const string query = "Select ANY_TABLE From Dual;";
            ExecuteNonQuery(query);
        }
    }
}
