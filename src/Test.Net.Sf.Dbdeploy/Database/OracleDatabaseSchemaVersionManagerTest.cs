namespace Net.Sf.Dbdeploy.Database
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.OracleClient;
    using System.Text;
    using NUnit.Framework;

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

        /// <summary>
        /// Ensures the table does not exist.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
		protected override void EnsureTableDoesNotExist(string tableName)
		{
			StringBuilder commandBuilder = new StringBuilder();
			commandBuilder.Append("Begin");
			commandBuilder.AppendFormat(" execute immediate 'DROP TABLE {0}';", tableName);
			commandBuilder.Append(" Exception when others then null;");
			commandBuilder.Append(" End;");

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