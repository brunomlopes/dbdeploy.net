using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Database
{
    [Category("MySQL"), Category("DbIntegration")]
    class MySqlDatabaseSchemaVersionManagerTest : AbstractDatabaseSchemaVersionManagerTest
    {
        private string connectionString;
        private const string DBMS = "mysql";
        private const string FOLDER = "Scripts";
        private readonly string mySqlDataDll = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mocks", "Fixtures", "MySqlDllConnection", "MySql.Data.dll");
        private IDbmsSyntax syntax;
        private readonly string[] CHANGELOG_TABLE_DOES_NOT_EXIST_MESSAGES = new[]
		{ 
            "No table found with name 'ChangeLog'.",
		};

        [SetUp]
        protected override void SetUp()
        {
            var factory = new DbmsFactory(Dbms, ConnectionString, mySqlDataDll);
            var executer = new QueryExecuter(factory);

            this.syntax = factory.CreateDbmsSyntax();
            databaseSchemaVersion = new DatabaseSchemaVersionManager(executer, this.syntax, TableName);
        }

        protected override string ConnectionString
        {
            get
            {
                if (connectionString == null)
                {
                    connectionString = ConfigurationManager.AppSettings["MySqlConnString-" + Environment.MachineName]
                                        ?? ConfigurationManager.AppSettings["MySqlConnString"];
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
            get { return CHANGELOG_TABLE_DOES_NOT_EXIST_MESSAGES; }
        }

        protected override string Dbms
        {
            get { return DBMS; }
        }

        [Test]
        public void ShouldNotThrowExceptionIfAllPreviousScriptsAreCompleted()
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

        public override void AssertTableExists(string tableName)
        {
            var schema = this.ExecuteScalar<string>(this.syntax.TableExists(tableName));

            Assert.IsNotNull(schema, string.Format("{0} table was not created.", tableName));
            Assert.IsNotEmpty(schema, string.Format("{0} table was not created.", tableName));
        }

        protected override void CreateTable()
        {
            if (!this.databaseSchemaVersion.ChangeLogTableExists())
            {
                var script = this.syntax.CreateChangeLogTableSqlScript(TableName);
                script = script.Replace(";", string.Empty);
                ExecuteSql(script);
            };
        }

        protected override IDbConnection GetConnection()
        {
            var pathDll = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mocks", "Fixtures", "MySqlDllConnection", mySqlDataDll);
            var assembly = Assembly.Load(AssemblyName.GetAssemblyName(pathDll).FullName);
            var type = assembly.GetType("MySql.Data.MySqlClient.MySqlConnection");
            return (IDbConnection)Activator.CreateInstance(type, ConnectionString);
        }

        protected override void InsertRowIntoTable(int i)
        {
            var commandBuilder = new StringBuilder();
            commandBuilder.AppendFormat("INSERT INTO {0}", TableName);
            commandBuilder.Append("(ChangeId, ScriptNumber, Folder, StartDate, CompleteDate, AppliedBy, ScriptName, ScriptStatus, ScriptOutput)");
            commandBuilder.AppendFormat(" VALUES (UUID(), {0}, '{1}', CURRENT_DATE, CURRENT_DATE, USER(), 'Unit test', 1, '')", i, FOLDER);
            ExecuteSql(commandBuilder.ToString());
        }
    }
}
