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
        private const string MySqlConnectionString = "Server=localhost; Database=dbdeploy; Uid=dbdeploy; Pwd=dbdeploy;";
        private string oracleDllPathVersion4, oracleDllPathVersion10, anyDll, otherOracleDllConnection, mySqlDllPath;
        private const string DbmsOracle = "ora";
        private const string DbmsMySql = "mysql";
        private const string OracleDllName = "Oracle.DataAccess.dll";
        private const string MySqlDllName = "MySql.Data.dll";

        [SetUp]
        public void SetUp()
        {
            mySqlDllPath = AppDomain.CurrentDomain.BaseDirectory + "\\Mocks\\Fixtures\\MySqlDllConnection\\" + MySqlDllName;
            oracleDllPathVersion4 = AppDomain.CurrentDomain.BaseDirectory + "\\Mocks\\Fixtures\\OracleDllConnection\\4.112.2.0\\" + OracleDllName;
            oracleDllPathVersion10 = AppDomain.CurrentDomain.BaseDirectory + "\\Mocks\\Fixtures\\OracleDllConnection\\10.2.0.100\\" + OracleDllName;
            anyDll = AppDomain.CurrentDomain.BaseDirectory + "\\Mocks\\Fixtures\\OracleDllConnection\\AnyDll.dll";
            otherOracleDllConnection = AppDomain.CurrentDomain.BaseDirectory + "\\Mocks\\Fixtures\\OracleDllConnection\\System.Data.OracleClient.dll";
        }

        [Test]
        [ExpectedException("System.IO.FileNotFoundException")]
        public void GivenAnEntryWithoutMySqlDllConnectionShouldThrowsException()
        {
            var factory = new DbmsFactory(DbmsMySql, MySqlConnectionString);
            var connection = OpenConnection(factory);
            Assert.AreEqual(ConnectionState.Open, connection.State);
            CloseConnection(connection);
        }

        [Test]
        public void GivenAnEntryWithMySqlDllConnectionShouldOpenedSuccessful()
        {
            var factory = new DbmsFactory(DbmsMySql, MySqlConnectionString, mySqlDllPath);
            var connection = OpenConnection(factory);
            Assert.AreEqual(ConnectionState.Open, connection.State);
            CloseConnection(connection);
        }

        [Test]
        [ExpectedException("System.BadImageFormatException")]
        public void GivenAnEntryWhereDllIsNotValidOracle()
        {
            var factory = new DbmsFactory(DbmsOracle, OracleConnectionStringTns, anyDll);
            var connection = OpenConnection(factory);
            CloseConnection(connection);
        }

        [Test]
        [ExpectedException("System.ArgumentNullException")]
        public void GivenAnEntryWhereDllIsNotValid2Oracle()
        {
            var factory = new DbmsFactory(DbmsOracle, OracleConnectionStringTns, otherOracleDllConnection);
            var connection = OpenConnection(factory);
            CloseConnection(connection);
        }

        [Test]
        public void GivenAnEntryWhereConnectionIsSucceededWithOracleDllPath()
        {
            var factory = new DbmsFactory(DbmsOracle, OracleConnectionStringTns, oracleDllPathVersion4);
            var connection = OpenConnection(factory);
            Assert.AreEqual(ConnectionState.Open, connection.State);
            CloseConnection(connection);
        }

        [Test]
        [ExpectedException("System.Reflection.TargetInvocationException")]
        public void GivenAnEntryWhereAnExceptionIsExpectedBecauseDllIsWrongOracle()
        {
            var factory = new DbmsFactory(DbmsOracle, OracleConnectionStringTns, oracleDllPathVersion10);
            var connection = OpenConnection(factory);
            CloseConnection(connection);
        }

        [Test]
        public void ShouldExecuteSqlCommandSuccessfulOracle()
        {
            var factory = new DbmsFactory(DbmsOracle, OracleConnectionStringTns, oracleDllPathVersion4);
            var connection = OpenConnection(factory);
            var command = connection.CreateCommand();
            command.CommandText = "Select Sysdate From Dual";
            //command.CommandText = "SELECT * FROM ChangeLog";//-- This commad works if table exist
            command.ExecuteNonQuery();
            CloseConnection(connection);
        }

        [Test]
        [ExpectedException("Oracle.DataAccess.Client.OracleException")]
        public void ShouldExecuteSqlCommandFailedOracle()
        {
            var factory = new DbmsFactory(DbmsOracle, OracleConnectionStringTns, oracleDllPathVersion4);
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