using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Npgsql;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Database
{
    [Category("PostgreSQL"), Category("DbIntegration")]
    class PostgresqlDatabaseSchemaVersionManagerTest : AbstractDatabaseSchemaVersionManagerTest
    {
        private string connectionString;
        private const string DBMS = "postgresql";
        private const string FOLDER = "Scripts";

        protected override void EnsureTableDoesNotExist(string tableName)
        {
            ExecuteSql("DROP TABLE IF EXISTS " + TableName);
        }

        protected override string ConnectionString
        {
            get
            {
                if (connectionString == null)
                {
                    connectionString = ConfigurationManager.AppSettings["PostgresSQLConnString-" + Environment.MachineName]
                                        ?? ConfigurationManager.AppSettings["PostgresSQLConnString"];
                }
                return connectionString;
            }
        }

        protected override string Folder
        {
            get { return FOLDER; }
        }

        protected override string[] ChangelogTableDoesNotExistMessages
        {
            get { return new string[] {}; }
        }

        protected override string Dbms
        {
            get { return DBMS; }
        }

        [Test]
        public  void ShouldNotThrowExceptionIfAllPreviousScriptsAreCompleted()
        {
            this.EnsureTableDoesNotExist();
            CreateTable();
            InsertRowIntoTable(3);
            var changeNumbers = new List<ChangeEntry>(databaseSchemaVersion.GetAppliedChanges());

            Assert.AreEqual(1, changeNumbers.Count);
            Assert.AreEqual("Scripts/3", changeNumbers[0].UniqueKey);
        }

        [Test]
        public override void TestCanRetrieveSchemaVersionFromDatabase()
        {
            base.TestCanRetrieveSchemaVersionFromDatabase();
        }

        [Test]
        public override void TestReturnsNoAppliedChangesWhenDatabaseTableDoesNotExist()
        {
            base.TestReturnsNoAppliedChangesWhenDatabaseTableDoesNotExist();
        }

        [Test]
        public override void TestShouldReturnEmptySetWhenTableHasNoRows()
        {
            base.TestShouldReturnEmptySetWhenTableHasNoRows();
        }

        [Test]
        public override void TestShouldCreateChangeLogTableWhenToldToDoSo()
        {
            base.TestShouldCreateChangeLogTableWhenToldToDoSo();
        }

        protected override IDbConnection GetConnection()
        {
            return new NpgsqlConnection(ConnectionString);
        }

        protected override void InsertRowIntoTable(int i)
        {
            var commandBuilder = new StringBuilder();
            commandBuilder.AppendFormat("INSERT INTO {0}", TableName);
            commandBuilder.Append("(ScriptNumber, Folder, StartDate, CompleteDate, AppliedBy, ScriptName, ScriptStatus, ScriptOutput)");
            commandBuilder.AppendFormat(" VALUES ({0}, '{1}', CURRENT_DATE, CURRENT_DATE, CURRENT_USER, 'Unit test', 1, '')", i, FOLDER);
            ExecuteSql(commandBuilder.ToString());
        }
    }
}
