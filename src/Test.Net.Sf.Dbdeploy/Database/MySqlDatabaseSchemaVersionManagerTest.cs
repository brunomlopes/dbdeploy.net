using MySql.Data.MySqlClient;

namespace Net.Sf.Dbdeploy.Database
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using global::Dbdeploy.Powershell;
    using Net.Sf.Dbdeploy.Appliers;
    using Net.Sf.Dbdeploy.Configuration;
    using Net.Sf.Dbdeploy.Database.SqlCmd;
    using Net.Sf.Dbdeploy.Exceptions;
    using Net.Sf.Dbdeploy.Scripts;
    using NUnit.Framework;

    [Category("MYSQL"), Category("DbIntegration")]
    public class MySqlDatabaseSchemaVersionManagerTest : AbstractDatabaseSchemaVersionManagerTest
    {
        private static string _connectionString;
        private const string FOLDER = "Scripts";

        private readonly string[] CHANGELOG_TABLE_DOES_NOT_EXIST_MESSAGES = new [] 
        {
            "No table found with name 'ChangeLog'.",
        };

		private const string DBMS = "mysql";

        protected override string ConnectionString
        {
            get
            {
                if (_connectionString == null)
                {
                    _connectionString = ConfigurationManager.AppSettings["MySqlConnString-" + Environment.MachineName]
                                        ?? ConfigurationManager.AppSettings["MySqlConnString"];
                }
                return _connectionString;
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
        public void TestDoesNotRunSecondScriptIfFirstScriptFails()
        {
            this.EnsureTableDoesNotExist("TableWeWillUse");
            this.EnsureTableDoesNotExist(TableName);

            var factory = new DbmsFactory(this.Dbms, this.ConnectionString);
            var dbmsSyntax = factory.CreateDbmsSyntax();

            var output = new StringBuilder();
            
            var applier = new TemplateBasedApplier(
                new StringWriter(output),
                dbmsSyntax,
                TableName,
                ";",
                DbDeployDefaults.DelimiterType,
                DbDeployDefaults.TemplateDirectory);

            applier.Apply(new ChangeScript[]
            {
                new StubChangeScript(1, "1.test.sql", "INSERT INTO TableWeWillUse VALUES (1);"), 
                new StubChangeScript(2, "2.test.sql", "CREATE TABLE dbo.TableWeWillUse (Id int NULL);"), 
            }, createChangeLogTable: true);

            using (var sqlExecuter = new SqlCmdExecutor(this.ConnectionString))
            {
                var cmdOutput = new StringBuilder();
                sqlExecuter.ExecuteString(output.ToString(), cmdOutput);
            }
            this.AssertTableDoesNotExist("TableWeWillUse");
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

        /// <summary>
        /// Ensures the table does not exist.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        protected override void EnsureTableDoesNotExist(string tableName)
        {
            var syntax = new MySqlDbmsSyntax();
            var tableInfo = syntax.GetTableInfo(tableName);
            this.ExecuteSql(string.Format(
                CultureInfo.InvariantCulture,
@"DROP TABLE IF EXISTS {0}.{1}", 
                tableInfo.Schema, tableInfo.TableName));
        }

        protected override IDbConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        protected override void InsertRowIntoTable(int i)
        {
            this.ExecuteSql("INSERT INTO " + TableName
                       + " (Folder, ScriptNumber, StartDate, CompleteDate, AppliedBy, ScriptName, ScriptStatus, ScriptOutput) VALUES ( "
                       + "'" + FOLDER + "', " + i
                       + ", CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, USER(), 'Unit test', 1, '')");
        }
    }
}