using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Dbdeploy.Powershell;
using Net.Sf.Dbdeploy.Appliers;
using Net.Sf.Dbdeploy.Configuration;
using Net.Sf.Dbdeploy.Database.SqlCmd;
using Net.Sf.Dbdeploy.Scripts;
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
            ExecuteSql("DROP TABLE IF EXISTS " + tableName);
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

        [Test]
        public void TestChangeDateIncludesTime()
        {
            this.EnsureTableDoesNotExist(TableName);

            var factory = new DbmsFactory(this.Dbms, this.ConnectionString);
            var dbmsSyntax = factory.CreateDbmsSyntax();

            var changeScripts = new ChangeScript[]
            {
                new StubChangeScript(1, "1.test.sql", "SELECT 1;"),
            };

            var queryExecuter = new QueryExecuter(factory);

            var doScriptApplier = new DirectToDbApplier(
                queryExecuter,
                databaseSchemaVersion,
                QueryStatementSplitter,
                dbmsSyntax,
                TableName,
                new LambdaTextWriter(s => { }));
            doScriptApplier.Apply(changeScripts, true);

            var now = DateTime.UtcNow;
            var date = ExecuteScalar<DateTime>("SELECT CompleteDate FROM {0} LIMIT 1", TableName);
            Assert.Less(Math.Abs((now - date).TotalMilliseconds), TimeSpan.FromSeconds(1).TotalMilliseconds);

        }

        [Test]
        public  void TestCanExecuteAnUpdateScript()
        {
            this.EnsureTableDoesNotExist("TableWeWillUse");
            this.EnsureTableDoesNotExist(TableName);

            var factory = new DbmsFactory(this.Dbms, this.ConnectionString);
            var dbmsSyntax = factory.CreateDbmsSyntax();

            var changeScripts = new ChangeScript[]
            {
                new StubChangeScript(1, "1.test.sql", "CREATE TABLE TableWeWillUse (Id int NULL);"),
                new StubChangeScript(2, "2.test.sql", "INSERT INTO TableWeWillUse VALUES (1);"),
            };

            var queryExecuter = new QueryExecuter(factory);


            var doScriptApplier = new DirectToDbApplier(
                queryExecuter,
                databaseSchemaVersion,
                QueryStatementSplitter,
                dbmsSyntax,
                TableName,
                new LambdaTextWriter(s => {}));

            var changeScriptRepository = new ChangeScriptRepository(changeScripts.ToList());

            var controller = new Controller(
                changeScriptRepository,
                databaseSchemaVersion,
                doScriptApplier,
                null,
                true,
                new LambdaTextWriter(s => { }));

            controller.ProcessChangeScripts(null);

            this.AssertTableExists("TableWeWillUse");
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

        private static QueryStatementSplitter QueryStatementSplitter = new QueryStatementSplitter
        {
            Delimiter = ";",
            DelimiterType = DbDeployDefaults.DelimiterType,
            LineEnding = DbDeployDefaults.LineEnding,
        };

    }
}
