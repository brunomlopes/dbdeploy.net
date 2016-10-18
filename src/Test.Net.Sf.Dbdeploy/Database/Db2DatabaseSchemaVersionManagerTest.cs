using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Text;
using Net.Sf.Dbdeploy.Appliers;
using Net.Sf.Dbdeploy.Configuration;
using Net.Sf.Dbdeploy.Scripts;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Database
{
	[Category("DB2"), Category("DbIntegration")]
	public class Db2DatabaseSchemaVersionManagerTest : AbstractDatabaseSchemaVersionManagerTest
	{
		private static string _connectionString;
		private const string FOLDER = "Scripts";

		private readonly string[] CHANGELOG_TABLE_DOES_NOT_EXIST_MESSAGES = new[] 
		{
			"No table found with name 'ChangeLog'.",
		};
		private const string DBMS = "db2";

		protected override string ConnectionString
		{
			get
			{
				if (_connectionString == null)
				{
					_connectionString = ConfigurationManager.AppSettings["Db2ConnString-" + Environment.MachineName]
										?? ConfigurationManager.AppSettings["Db2ConnString"];
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

		protected override IDbConnection GetConnection()
		{
			return new Wintegra.Data.Db2Client.Db2Connection(ConnectionString);
		}

		protected override void InsertRowIntoTable(int i)
		{
			StringBuilder commandBuilder = new StringBuilder();
			commandBuilder.AppendFormat("INSERT INTO {0}", TableName);
			commandBuilder.Append("(ScriptNumber, Folder, StartDate, CompleteDate, AppliedBy, ScriptName, ScriptStatus, ScriptOutput)");
			commandBuilder.AppendFormat(" VALUES ({0}, '{1}', CURRENT TIMESTAMP, CURRENT TIMESTAMP, CURRENT USER, 'Unit test', 1, '')", i, FOLDER);
			ExecuteSql(commandBuilder.ToString());
		}

		/// <summary>
		/// Ensures the table does not exist.
		/// </summary>
		/// <param name="tableName">Name of the table.</param>
		protected override void EnsureTableDoesNotExist(string tableName)
		{
			var schema = this.ExecuteScalar<string>(this.syntax.TableExists(tableName));
			if (string.IsNullOrEmpty(schema)) return;

			var commandBuilder = new StringBuilder();
			commandBuilder.AppendFormat("DROP TABLE {0}", schema);
			ExecuteSql(commandBuilder.ToString());
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
				new StubChangeScript(2, "2.test.sql", "CREATE TABLE TableWeWillUse (Id int NULL);"), 
			}, createChangeLogTable: true);

			var cmdOutput = new StringBuilder();

			var executer = new QueryExecuter(factory);
			executer.Execute(output.ToString(), cmdOutput);

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
	}
}