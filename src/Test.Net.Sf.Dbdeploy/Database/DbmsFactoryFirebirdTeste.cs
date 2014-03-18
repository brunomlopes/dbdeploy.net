using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Database
{
    class DbmsFactoryFirebirdTeste : DbmsFactoryAbstractTest
    {
        private readonly string firebirdConnectionString = string.Format("User=SYSDBA;Password=masterkey;Database={0};DataSource=localhost;Port=3050;", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mocks", "Fixtures", "DatabaseFirebird", "DBDEPLOY15.FDB"));
        private const string DbmsFirebird = "firebird";
        private const string FirebirdDllName = "FirebirdSql.Data.FirebirdClient.dll";
        private readonly string firebirdDllPath = AppDomain.CurrentDomain.BaseDirectory + "\\Mocks\\Fixtures\\FirebirdDllConnection\\" + FirebirdDllName;

        [Test]
        public override void GivenAnEntryWithoutDllConnectionShouldThrowsException()
        {
            var factory = new DbmsFactory(DbmsFirebird, firebirdConnectionString);
            var connection = OpenConnection(factory);
            Assert.AreEqual(ConnectionState.Open, connection.State);
            CloseConnection();
        }

        [Test]
        public override void GivenAnEntryWithDllConnectionShouldOpenedSuccessful()
        {
            var factory = new DbmsFactory(DbmsFirebird, firebirdConnectionString, firebirdDllPath);
            var connection = OpenConnection(factory);
            Assert.AreEqual(ConnectionState.Open, connection.State);
            CloseConnection();
        }

        [Test]
        public override void ShouldExecuteSqlCommandSuccessful()
        {
            var factory = new DbmsFactory(DbmsFirebird, firebirdConnectionString, firebirdDllPath);
            OpenConnection(factory);
            const string query = "SELECT * FROM rdb$relation_fields";
            ExecuteNonQuery(query);
            CloseConnection();
        }

        [Test]
        [ExpectedException("FirebirdSql.Data.FirebirdClient.FbException")]
        public override void ShouldExecuteSqlCommandFailed()
        {
            var factory = new DbmsFactory(DbmsFirebird, firebirdConnectionString, firebirdDllPath);
            OpenConnection(factory);
            const string query = "SELECT * FROM ANY_TABLE";
            ExecuteNonQuery(query);
            CloseConnection();
        }
    }
}
