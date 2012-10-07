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
		private const string DELTA_SET = "All";
		private readonly string[] CHANGELOG_TABLE_DOES_NOT_EXIST_MESSAGES = new []
		{ 
			"Could not retrieve change log from database because: ORA-00942: table or view does not exist\n" 
		};
		private const string DBMS = "ora";

		protected override string ConnectionString
		{
			get { return CONNECTION_STRING; }
		}

		protected override string DeltaSet
		{
			get { return DELTA_SET; }
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
			commandBuilder.Append(" 'CREATE TABLE changelog (");
			commandBuilder.Append(" change_number INTEGER NOT NULL,");
			commandBuilder.Append(" delta_set VARCHAR2(10) NOT NULL,");
			commandBuilder.Append(" start_dt TIMESTAMP NOT NULL,");
			commandBuilder.Append(" complete_dt TIMESTAMP NULL,");
			commandBuilder.Append(" applied_by VARCHAR2(100) NOT NULL,");
			commandBuilder.Append(" description VARCHAR2(500) NOT NULL");
			commandBuilder.Append(" )';");
			commandBuilder.Append(" END;");
			ExecuteSql(commandBuilder.ToString());

			commandBuilder = new StringBuilder();
			commandBuilder.Append("BEGIN execute immediate");
			commandBuilder.Append(" 'ALTER TABLE changelog ADD CONSTRAINT Pkchangelog PRIMARY KEY (change_number, delta_set)';");
			commandBuilder.Append(" END;");
			ExecuteSql(commandBuilder.ToString());
		}

		protected override void InsertRowIntoTable(int i)
		{
			StringBuilder commandBuilder = new StringBuilder();
			commandBuilder.AppendFormat("INSERT INTO {0}", TableName);
			commandBuilder.Append("(change_number, delta_set, start_dt, complete_dt, applied_by, description)");
			commandBuilder.AppendFormat(" VALUES ({0}, '{1}', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, USER, 'Unit test')", i, DELTA_SET);
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
	}
}