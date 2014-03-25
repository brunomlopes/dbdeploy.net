using System;
using System.Data;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Database
{
    /// <summary>
    /// Tests for CORE version 11.2.0.2.0 Production Oracle Application Express - XE
    /// The tnsnames.ora file is default config
    /// </summary>
    class DbmsFactoryOracleTest : DbmsFactoryAbstractTest
    {
        private const string OracleConnectionStringTns = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=XE)));User Id=dbdeploy;Password=dbdeploy;";
        private readonly string oracleDllConnection = AppDomain.CurrentDomain.BaseDirectory + "\\Mocks\\Fixtures\\OracleDllConnection\\" + OracleDllName;
        private const string DbmsOracle = "ora";
        private const string OracleDllName = "Oracle.DataAccess.dll";

        [Test]
        public override void GivenAnEntryWithoutDllConnectionShouldThrowsException()
        {
            var factory = new DbmsFactory(DbmsOracle, OracleConnectionStringTns);
            var connection = OpenConnection(factory);
            Assert.AreEqual(ConnectionState.Open, connection.State);
        }

        [Test]
        public override void GivenAnEntryWithDllConnectionShouldOpenedSuccessful()
        {
            var factory = new DbmsFactory(DbmsOracle, OracleConnectionStringTns, oracleDllConnection);
            var connection = OpenConnection(factory);
            Assert.AreEqual(ConnectionState.Open, connection.State);
        }

        [Test]
        public override void ShouldExecuteSqlCommandSuccessful()
        {
            var factory = new DbmsFactory(DbmsOracle, OracleConnectionStringTns, oracleDllConnection);
            OpenConnection(factory);
            const string query = "Select Sysdate From Dual";
            ExecuteNonQuery(query);
        }

        [Test]
        [ExpectedException("Oracle.DataAccess.Client.OracleException")]
        public override void ShouldExecuteSqlCommandFailed()
        {
            var factory = new DbmsFactory(DbmsOracle, OracleConnectionStringTns, oracleDllConnection);
            OpenConnection(factory);
            const string query = "Select * from ANY_TABLE";
            ExecuteNonQuery(query);
        }
    }
}