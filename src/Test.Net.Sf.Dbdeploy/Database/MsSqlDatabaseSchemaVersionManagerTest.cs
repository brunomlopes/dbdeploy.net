using FluentAssertions;

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
    using Appliers;
    using Configuration;
    using SqlCmd;
    using Scripts;
    using NUnit.Framework;

    [Category("MSSQL"), Category("DbIntegration")]
    public class MsSqlDatabaseSchemaVersionManagerTest : AbstractDatabaseSchemaVersionManagerTest
    {
        private static string _connectionString;
        private const string FOLDER = "Scripts";

        protected override string ConnectionString
        {
            get
            {
                if (_connectionString == null)
                {
                    _connectionString = ConfigurationManager.AppSettings["ConnString-" + Environment.MachineName]
                                        ?? ConfigurationManager.AppSettings["ConnString"];
                }
                return _connectionString;
            }
        }

        protected override string Folder
        {
            get { return FOLDER; }
        }

    	protected override string Dbms
    	{
			get { return SupportedDbms.MSSQL; }
    	}

        [Test]
        public override void ShouldNotThrowExceptionIfAllPreviousScriptsAreCompleted()
    	{
			EnsureTableDoesNotExist();
			CreateTable();
    		InsertRowIntoTable(3);
			var changeNumbers = new List<ChangeEntry>(databaseSchemaVersion.GetAppliedChanges());

			Assert.AreEqual(1, changeNumbers.Count);
			Assert.AreEqual("Scripts/3", changeNumbers[0].UniqueKey);
		}


        [Test]
        public void TestDoesNotRunSecondScriptIfFirstScriptFails()
        {
            EnsureTableDoesNotExist("TableWeWillUse");
            EnsureTableDoesNotExist(TableName);

            var factory = new DbmsFactory(Dbms, ConnectionString);
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
            }, true);

            using (var sqlExecuter = new SqlCmdExecutor(ConnectionString))
            {
                var cmdOutput = new StringBuilder();
                sqlExecuter.ExecuteString(output.ToString(), cmdOutput);
            }
            AssertTableDoesNotExist("TableWeWillUse");
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
            this.EnsureTableDoesNotExist();

            // Create change log table
            CreateTable();

            AssertTableExist("ChangeLog");
        }

        public void AssertTableExist(string tableName)
        {
            var schema = this.ExecuteScalar<int>(this.syntax.TableExists(tableName));

            schema.Should().BeGreaterThan(0);
        }

        /// <summary>
        /// Ensures the table does not exist.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        protected override void EnsureTableDoesNotExist(string tableName)
        {
            var syntax = new MsSqlDbmsSyntax();
            var tableInfo = syntax.GetTableInfo(tableName);
            ExecuteSql(string.Format(
                CultureInfo.InvariantCulture,
@"IF (EXISTS (SELECT * 
    FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_NAME = '{0}'))
BEGIN
    DROP Table {0}
END", tableInfo.TableName));
        }

        protected override IDbConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public override void AssertTableDoesNotExist(string tableName)
        {
            var schema = this.ExecuteScalar<int>(this.syntax.TableExists(tableName));

            Assert.IsNull(schema, string.Format("{0} table was created.", tableName));
        }

        protected override void InsertRowIntoTable(int i)
        {
            ExecuteSql("INSERT INTO " + TableName
                       + " (ChangeId, Folder, ScriptNumber, StartDate, CompleteDate, AppliedBy, ScriptName, ScriptStatus, ScriptOutput) VALUES (newid(), "
                       + "'" + FOLDER + "', " + i
                       + ", getdate(), getdate(), user_name(), 'Unit test', 1, '')");
        }
    }
}