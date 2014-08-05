using System;
using System.Data;
using System.IO;
using Net.Sf.Dbdeploy.Configuration;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Database
{
    class DbmsFactoryFirebirdTest : DbmsFactoryAbstractTest
    {
        private readonly string firebirdConnectionString = string.Format("User=SYSDBA;Password=masterkey;Database={0};DataSource=localhost;Port=3050;", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mocks", "Fixtures", "DatabaseFirebird", "DBDEPLOY.FDB"));
        private const string FirebirdDllName = "FirebirdSql.Data.FirebirdClient.dll";
        private readonly string firebirdDllPath = AppDomain.CurrentDomain.BaseDirectory + "\\Mocks\\Fixtures\\FirebirdDllConnection\\" + FirebirdDllName;

        [Test]
        public override void GivenAnEntryWithoutDllConnectionShouldThrowsException()
        {
            var factory = new DbmsFactory(BancosSuportados.FIREBIRD, firebirdConnectionString);
            var connection = OpenConnection(factory);
            Assert.AreEqual(ConnectionState.Open, connection.State);
        }

        [Test]
        public override void GivenAnEntryWithDllConnectionShouldOpenedSuccessful()
        {
            var factory = new DbmsFactory(BancosSuportados.FIREBIRD, firebirdConnectionString, firebirdDllPath);
            var connection = OpenConnection(factory);
            Assert.AreEqual(ConnectionState.Open, connection.State);
        }

        [Test]
        public override void ShouldExecuteSqlCommandSuccessful()
        {
            var factory = new DbmsFactory(BancosSuportados.FIREBIRD, firebirdConnectionString, firebirdDllPath);
            OpenConnection(factory);
            const string query = "SELECT * FROM rdb$relation_fields";
            ExecuteNonQuery(query);
        }

        [Test]
        [ExpectedException("FirebirdSql.Data.FirebirdClient.FbException")]
        public override void ShouldExecuteSqlCommandFailed()
        {
            var factory = new DbmsFactory(BancosSuportados.FIREBIRD, firebirdConnectionString, firebirdDllPath);
            OpenConnection(factory);
            const string query = "SELECT * FROM ANY_TABLE";
            ExecuteNonQuery(query);
        }
    }
}
