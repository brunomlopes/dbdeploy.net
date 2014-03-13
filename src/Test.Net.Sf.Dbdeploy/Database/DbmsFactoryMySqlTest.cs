using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Database
{
    class DbmsFactoryMySqlTest : DbmsFactoryAbstractTest
    {
        private readonly string mySqlDllPath = AppDomain.CurrentDomain.BaseDirectory + "\\Mocks\\Fixtures\\MySqlDllConnection\\" + MySqlDllName;
        private const string DbmsMySql = "mysql";
        private const string MySqlDllName = "MySql.Data.dll";
        private const string MySqlConnectionString = "Server=localhost; Database=dbdeploy; Uid=dbdeploy; Pwd=dbdeploy;";

        [Test]
        [ExpectedException("System.IO.FileNotFoundException")]
        public override void GivenAnEntryWithoutDllConnectionShouldThrowsException()
        {
            var factory = new DbmsFactory(DbmsMySql, MySqlConnectionString);
            var connection = OpenConnection(factory);
            Assert.AreEqual(ConnectionState.Open, connection.State);
            CloseConnection();
        }

        [Test]
        public override void GivenAnEntryWithDllConnectionShouldOpenedSuccessful()
        {
            var factory = new DbmsFactory(DbmsMySql, MySqlConnectionString, mySqlDllPath);
            var connection = OpenConnection(factory);
            Assert.AreEqual(ConnectionState.Open, connection.State);
            CloseConnection();
        }

        [Test]
        public override void ShouldExecuteSqlCommandSuccessful()
        {
            var factory = new DbmsFactory(DbmsMySql, MySqlConnectionString, mySqlDllPath);
            OpenConnection(factory);
            const string query = "Select CURRENT_TIMESTAMP From Dual;";
            ExecuteNonQuery(query);
            CloseConnection();
        }

        [Test]
        [ExpectedException("MySql.Data.MySqlClient.MySqlException")]
        public override void ShouldExecuteSqlCommandFailed()
        {
            var factory = new DbmsFactory(DbmsMySql, MySqlConnectionString, mySqlDllPath);
            OpenConnection(factory);
            const string query = "Select ANY_TABLE From Dual;";
            ExecuteNonQuery(query);
            CloseConnection();
        }
    }
}