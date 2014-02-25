using System;
using System.Data;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Database
{
    /// <summary>
    /// Tests for CORE version 11.2.0.2.0 Production Oracle Application Express - XE
    /// The tnsnames.ora file is default config
    /// </summary>
    class DbmsFactoryTest
    {
        private const string OracleConnectionStringTns = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=XE)));User Id=DBDEPLOY;Password=DBDEPLOY;";
        private string oracleDllPathVersion4, oracleDllPathVersion10, anyDll, otherOracleDllConnection;
        private const string Dbms = "ora";
        private const string OracleDllName = "Oracle.DataAccess.dll";

        [SetUp]
        public void SetUp()
        {
            oracleDllPathVersion4 = AppDomain.CurrentDomain.BaseDirectory + "\\Mocks\\Fixtures\\OracleDllConnection\\4.112.2.0\\" + OracleDllName;
            oracleDllPathVersion10 = AppDomain.CurrentDomain.BaseDirectory + "\\Mocks\\Fixtures\\OracleDllConnection\\10.2.0.100\\" + OracleDllName;
            anyDll = AppDomain.CurrentDomain.BaseDirectory + "\\Mocks\\Fixtures\\OracleDllConnection\\AnyDll.dll";
            otherOracleDllConnection = AppDomain.CurrentDomain.BaseDirectory + "\\Mocks\\Fixtures\\OracleDllConnection\\System.Data.OracleClient.dll";
        }

        [Test]
        public void GivenAnEntryWithoutOracleDllPathShouldGetDefaultConfigOnDbdeploy()
        {
            var factory = new DbmsFactory(Dbms, OracleConnectionStringTns);
            var connection = OpenConnection(factory);
            Assert.AreEqual(ConnectionState.Open, connection.State);
            CloseConnection(connection);
        }

        [Test]
        [ExpectedException("System.BadImageFormatException")]
        public void GivenAnEntryWhereDllIsNotValid()
        {
            var factory = new DbmsFactory(Dbms, OracleConnectionStringTns, anyDll);
            var connection = OpenConnection(factory);
            CloseConnection(connection);
        }

        [Test]
        [ExpectedException("System.ArgumentNullException")]
        public void GivenAnEntryWhereDllIsNotValid2()
        {
            var factory = new DbmsFactory(Dbms, OracleConnectionStringTns, otherOracleDllConnection);
            var connection = OpenConnection(factory);
            CloseConnection(connection);
        }

        [Test]
        public void GivenAnEntryWhereConnectionIsSucceededWithOracleDllPath()
        {
            var factory = new DbmsFactory(Dbms, OracleConnectionStringTns, oracleDllPathVersion4);
            var connection = OpenConnection(factory);
            Assert.AreEqual(ConnectionState.Open, connection.State);
            CloseConnection(connection);
        }

        [Test]
        [ExpectedException("System.Reflection.TargetInvocationException")]
        public void GivenAnEntryWhereAnExceptionIsExpectedBecauseDllIsWrong()
        {
            var factory = new DbmsFactory(Dbms, OracleConnectionStringTns, oracleDllPathVersion10);
            var connection = OpenConnection(factory);
            CloseConnection(connection);
        }

        [Test]
        public void ShouldExecuteSqlCommandSuccessful()
        {
            var factory = new DbmsFactory(Dbms, OracleConnectionStringTns, oracleDllPathVersion4);
            var connection = OpenConnection(factory);
            var command = connection.CreateCommand();
            command.CommandText = "Select Sysdate From Dual";
            //command.CommandText = "SELECT * FROM ChangeLog";//-- This commad works if table exist
            command.ExecuteNonQuery();
            CloseConnection(connection);
        }

        [Test]
        [ExpectedException("Oracle.DataAccess.Client.OracleException")]
        public void ShouldExecuteSqlCommandFailed()
        {
            var factory = new DbmsFactory(Dbms, OracleConnectionStringTns, oracleDllPathVersion4);
            var connection = OpenConnection(factory);
            var command = connection.CreateCommand();
            command.CommandText = "Select * From ANY_TABLE";
            command.ExecuteNonQuery();
            CloseConnection(connection);
        }

        private IDbConnection OpenConnection(DbmsFactory factory)
        {
            var connection = factory.CreateConnection();
            connection.Open();
            return connection;
        }

        private void CloseConnection(IDbConnection connection)
        {
            connection.Close();
        }
    }
}