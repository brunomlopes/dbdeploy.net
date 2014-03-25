using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Database
{
    class DbmsFactoryPostgresTest : DbmsFactoryAbstractTest
    {
        private const string PostgresConnectionString = "Server=localhost;Port=5432;Database=dbdeploy;User Id=postgres;Password=postgres;";
        private const string DbmsPostgres = "postgres";
        private const string PostgresDllName = "Npgsql.dll";
        private readonly string postGresDllPath = AppDomain.CurrentDomain.BaseDirectory + "\\Mocks\\Fixtures\\PostgresDllConnection\\" + PostgresDllName;

        [Test]
        [ExpectedException("System.IO.FileNotFoundException")]
        public override void GivenAnEntryWithoutDllConnectionShouldThrowsException()
        {
            var factory = new DbmsFactory(DbmsPostgres, PostgresConnectionString);
            var connection = OpenConnection(factory);
            Assert.AreEqual(ConnectionState.Open, connection.State);
        }

        [Test]
        public override void GivenAnEntryWithDllConnectionShouldOpenedSuccessful()
        {
            var factory = new DbmsFactory(DbmsPostgres, PostgresConnectionString, postGresDllPath);
            var connection = OpenConnection(factory);
            Assert.AreEqual(ConnectionState.Open, connection.State);
        }

        [Test]
        public override void ShouldExecuteSqlCommandSuccessful()
        {
            var factory = new DbmsFactory(DbmsPostgres, PostgresConnectionString, postGresDllPath);
            OpenConnection(factory);
            const string query = "SELECT CURRENT_DATE;";
            ExecuteNonQuery(query);
        }

        [Test]
        [ExpectedException("Npgsql.NpgsqlException")]
        public override void ShouldExecuteSqlCommandFailed()
        {
            var factory = new DbmsFactory(DbmsPostgres, PostgresConnectionString, postGresDllPath);
            OpenConnection(factory);
            const string query = "Select ANY_TABLE From Dual;";
            ExecuteNonQuery(query);
        }
    }
}
