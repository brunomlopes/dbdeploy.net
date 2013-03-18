using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OracleClient;
using System.Text;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Database
{
	[Category("Oracle"), Category("DbIntegration")]
	public class OracleDatabaseSchemaVersionManagerTest : AbstractDatabaseSchemaVersionManagerTest
	{
		private static readonly string CONNECTION_STRING = ConfigurationManager.AppSettings["OracleConnString"];
		private const string FOLDER = "Scripts";
		private readonly string[] CHANGELOG_TABLE_DOES_NOT_EXIST_MESSAGES = new []
		{ 
            "No table found with name 'ChangeLog'.",
		};
		private const string DBMS = "ora";

		protected override string ConnectionString
		{
			get { return CONNECTION_STRING; }
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

		protected override void CreateTable()
		{
			StringBuilder commandBuilder = new StringBuilder();
			commandBuilder.Append("BEGIN execute immediate");
            commandBuilder.Append(" 'CREATE TABLE ChangeLog (");
			commandBuilder.Append(" ScriptNumber INTEGER NOT NULL,");
			commandBuilder.Append(" Folder VARCHAR2(256) NOT NULL,");
			commandBuilder.Append(" StartDate TIMESTAMP NOT NULL,");
			commandBuilder.Append(" CompleteDate TIMESTAMP NULL,");
			commandBuilder.Append(" AppliedBy VARCHAR2(128) NOT NULL,");
			commandBuilder.Append(" FileName VARCHAR2(512) NOT NULL");
			commandBuilder.Append(" )';");
			commandBuilder.Append(" END;");
			ExecuteSql(commandBuilder.ToString());

			commandBuilder = new StringBuilder();
			commandBuilder.Append("BEGIN execute immediate");
            commandBuilder.Append(" 'ALTER TABLE ChangeLog ADD CONSTRAINT PK_ChangeLog PRIMARY KEY (ScriptNumber, Folder)';");
			commandBuilder.Append(" END;");
			ExecuteSql(commandBuilder.ToString());
		}

		protected override void InsertRowIntoTable(int i)
		{
			StringBuilder commandBuilder = new StringBuilder();
			commandBuilder.AppendFormat("INSERT INTO {0}", TableName);
			commandBuilder.Append("(ScriptNumber, Folder, StartDate, CompleteDate, AppliedBy, FileName)");
			commandBuilder.AppendFormat(" VALUES ({0}, '{1}', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, USER, 'Unit test')", i, FOLDER);
			ExecuteSql(commandBuilder.ToString());
		}

		protected override IDbConnection GetConnection()
		{
			return new OracleConnection(CONNECTION_STRING);
		}

		protected override void EnsureTableDoesNotExist()
		{
			StringBuilder commandBuilder = new StringBuilder();
			commandBuilder.Append("Begin");
			commandBuilder.AppendFormat(" execute immediate 'DROP TABLE {0}';", TableName);
			commandBuilder.Append(" Exception when others then null;");
			commandBuilder.Append(" End;");

			ExecuteSql(commandBuilder.ToString());
		}

		[Test]
		public void ShouldNotThrowExceptionIfAllPreviousScriptsAreCompleted()
		{
			EnsureTableDoesNotExist();
			CreateTable();
			InsertRowIntoTable(3);
			List<int> changeNumbers = new List<int>(databaseSchemaVersion.GetAppliedChanges());

			Assert.AreEqual(1, changeNumbers.Count);
			Assert.AreEqual(3, changeNumbers[0]);
		}

		[Test]
		public override void TestCanRetrieveSchemaVersionFromDatabase()
		{
			base.TestCanRetrieveSchemaVersionFromDatabase();
		}

		[Test]
		public override void TestThrowsWhenDatabaseTableDoesNotExist()
		{
			base.TestThrowsWhenDatabaseTableDoesNotExist();
		}

		[Test]
		public override void TestShouldReturnEmptySetWhenTableHasNoRows()
		{
			base.TestShouldReturnEmptySetWhenTableHasNoRows();
		}

        [Test]
        public override void TestShouldCreateChangeLogTableWhenDoesNotExist()
        {
            base.TestShouldCreateChangeLogTableWhenDoesNotExist();
        }
	}
}