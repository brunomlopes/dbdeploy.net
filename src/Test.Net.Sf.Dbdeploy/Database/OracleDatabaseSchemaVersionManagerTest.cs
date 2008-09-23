using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Text;
using Net.Sf.Dbdeploy.Exceptions;
using Net.Sf.Dbdeploy.Scripts;
using NUnit.Framework;
using System.Data.OracleClient;

namespace Net.Sf.Dbdeploy.Database
{
	[Category("Oracle")]
	public class OracleDatabaseSchemaVersionManagerTest : AbstractDatabaseSchemaVersionManagerTest
	{
		private static readonly string CONNECTION_STRING = ConfigurationManager.AppSettings["OracleConnString"];
		private const string DELTA_SET = "All";
		private const string CHANGELOG_TABLE_DOES_NOT_EXIST_MESSAGE = "Could not retrieve change log from database because: ORA-00942: table or view does not exist\n";
		private const string DBMS = "ora";

		protected override string ConnectionString
		{
			get { return CONNECTION_STRING; }
		}

		protected override string DeltaSet
		{
			get { return DELTA_SET; }
		}

		protected override string ChangelogTableDoesNotExistMessage
		{
			get { return CHANGELOG_TABLE_DOES_NOT_EXIST_MESSAGE; }
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
			commandBuilder.AppendFormat("INSERT INTO {0}", databaseSchemaVersion.TableName);
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
			commandBuilder.AppendFormat(" execute immediate 'DROP TABLE {0}';", databaseSchemaVersion.TableName);
			commandBuilder.Append(" Exception when others then null;");
			commandBuilder.Append(" End;");

			ExecuteSql(commandBuilder.ToString());
		}

		[Test]
		[ExpectedException(typeof(DbDeployException))]
		public void ShouldThrowExceptionIfIncompletedScriptIsFound()
		{
			EnsureTableDoesNotExist();
			CreateTable();
			ExecuteSql("INSERT INTO " + databaseSchemaVersion.TableName
					   + " (change_number, delta_set, start_dt, complete_dt, applied_by, description) VALUES ( "
					   + 1 + ", '" + DELTA_SET
					   + "', CURRENT_TIMESTAMP, NULL, USER, 'Unit test')");

			databaseSchemaVersion.GetAppliedChangeNumbers();
		}

		[Test]
		public void ShouldNotThrowExceptionIfAllPreviousScriptsAreCompleted()
		{
			EnsureTableDoesNotExist();
			CreateTable();
			InsertRowIntoTable(3);
			List<int> changeNumbers = databaseSchemaVersion.GetAppliedChangeNumbers();

			Assert.AreEqual(1, changeNumbers.Count);
			Assert.AreEqual(3, changeNumbers[0]);
		}

		[Test]
		public override void TestCanRetrieveDeltaFragmentFooterSql()
		{
			ChangeScript script = new ChangeScript(3, "description");
			Assert.AreEqual(
				@"UPDATE changelog SET complete_dt = CURRENT_TIMESTAMP WHERE change_number = 3 AND delta_set = 'All';
COMMIT;
--------------- Fragment ends: #3 ---------------",
				databaseSchemaVersion.GenerateDoDeltaFragmentFooter(script));
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
		public override void TestCanRetrieveDeltaFragmentHeaderSql()
		{
			ChangeScript script = new ChangeScript(3, "description");
			Assert.AreEqual(@"--------------- Fragment begins: #3 ---------------
INSERT INTO changelog (change_number, delta_set, start_dt, applied_by, description) VALUES (3, 'All', CURRENT_TIMESTAMP, USER, 'description');
COMMIT;",
				databaseSchemaVersion.GenerateDoDeltaFragmentHeader(script));
		}

		[Test]
		public override void TestCanSetChangeLogTableName()
		{
			base.TestCanSetChangeLogTableName();
		}

		[Test]
		public void TestCanGenerateVersionCheck()
		{
			databaseSchemaVersion = new DatabaseSchemaVersionManager(new DbmsFactory(DBMS, CONNECTION_STRING), "Main", 5);
			Assert.AreEqual(@"execute versionCheck('Main', 5, 'changelog');"
			, databaseSchemaVersion.GenerateVersionCheck());
		}

	}
}