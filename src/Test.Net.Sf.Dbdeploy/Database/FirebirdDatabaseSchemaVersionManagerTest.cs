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
    class FirebirdDatabaseSchemaVersionManagerTest : AbstractDatabaseSchemaVersionManagerTest
    {
        private string connectionString;
        private const string DBMS = "firebird";
        private const string FOLDER = "Scripts";
        private readonly string firebirdSqlDataFirebirdClient = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mocks", "Fixtures", "FirebirdDllConnection", "FirebirdSql.Data.FirebirdClient.dll");
        private IDbmsSyntax syntax;

        [SetUp]
        protected override void SetUp()
        {
            var factory = new DbmsFactory(Dbms, ConnectionString, firebirdSqlDataFirebirdClient);
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
                    connectionString = ConfigurationManager.AppSettings["FirebirdConnString-" + Environment.MachineName]
                                        ?? ConfigurationManager.AppSettings["FirebirdConnString"];
                }
                return connectionString;
            }
        }

        protected override string Folder
        {
            get { return FOLDER; }
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
                ExecuteSql(script);
            };
        }

        protected override IDbConnection GetConnection()
        {
            var assembly = Assembly.Load(AssemblyName.GetAssemblyName(firebirdSqlDataFirebirdClient).FullName);
            var type = assembly.GetType("FirebirdSql.Data.FirebirdClient.FbConnection");
            return (IDbConnection)Activator.CreateInstance(type, ConnectionString);
        }

        protected override void InsertRowIntoTable(int i)
        {
            var commandBuilder = new StringBuilder();
            commandBuilder.AppendFormat("INSERT INTO {0}", TableName);
            commandBuilder.Append("(ChangeId, ScriptNumber, Folder, StartDate, CompleteDate, AppliedBy, ScriptName, ScriptStatus, ScriptOutput)");
            commandBuilder.AppendFormat(" VALUES ('{0}', {1}, '{2}', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, CURRENT_USER, 'Unit test', 1, 'Any')", Guid.NewGuid(), i, FOLDER);
            ExecuteSql(commandBuilder.ToString());
        }
    }
}