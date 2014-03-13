using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Database
{
    class DbmsFactoryFirebirdTeste : DbmsFactoryAbstractTest
    {
        private const string FirebirdConnectionString = "User=SYSDBA;Password=masterkey;Database=C:\\DBDEPLOY.fdb;DataSource=localhost;Port=3050;";
        private const string DbmsFirebird = "firebird";
        private const string FirebirdDllName = "FirebirdSql.Data.FirebirdClient.dll";
        private readonly string firebirdDllPath = AppDomain.CurrentDomain.BaseDirectory + "\\Mocks\\Fixtures\\FirebirdDllConnection\\" + FirebirdDllName;

        [Test]
        public override void GivenAnEntryWithoutDllConnectionShouldThrowsException()
        {
            var factory = new DbmsFactory(DbmsFirebird, FirebirdConnectionString);
            var connection = OpenConnection(factory);
            Assert.AreEqual(ConnectionState.Open, connection.State);
            CloseConnection();
        }

        [Test]
        public override void GivenAnEntryWithDllConnectionShouldOpenedSuccessful()
        {
            var factory = new DbmsFactory(DbmsFirebird, FirebirdConnectionString, firebirdDllPath);
            var connection = OpenConnection(factory);
            Assert.AreEqual(ConnectionState.Open, connection.State);
            CloseConnection();
        }

        [Test]
        public override void ShouldExecuteSqlCommandSuccessful()
        {
            var factory = new DbmsFactory(DbmsFirebird, FirebirdConnectionString, firebirdDllPath);
            OpenConnection(factory);
            const string query = "SELECT * FROM rdb$relation_fields";
            ExecuteNonQuery(query);
            CloseConnection();
        }

        [Test]
        [ExpectedException("FirebirdSql.Data.FirebirdClient.FbException")]
        public override void ShouldExecuteSqlCommandFailed()
        {
            var factory = new DbmsFactory(DbmsFirebird, FirebirdConnectionString, firebirdDllPath);
            OpenConnection(factory);
            const string query = "SELECT * FROM ANY_TABLE";
            ExecuteNonQuery(query);
            CloseConnection();
        }
    }
}
