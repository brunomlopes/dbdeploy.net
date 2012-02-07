using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Database
{
    [Category("MSSQL"), Category("DbIntegration")]
    public class MsSqlDatabaseSchemaVersionManagerTest : AbstractDatabaseSchemaVersionManagerTest
    {
        private static readonly string CONNECTION_STRING = ConfigurationManager.AppSettings["ConnString"];
        private const string DELTA_SET = "All";

        private readonly string[] CHANGELOG_TABLE_DOES_NOT_EXIST_MESSAGES = new [] 
        {
            "Could not retrieve change log from database because: Ungültiger Objektname 'changelog'.",  // German
            "Could not retrieve change log from database because: Invalid object name 'changelog'."     // English
        };

		private const string DBMS = "mssql";

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

    	protected override void EnsureTableDoesNotExist()
        {
            ExecuteSql(string.Format(
				"IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[{0}]') AND type in (N'U')) DROP TABLE [{0}]", TableName));
        }

    	protected override IDbConnection GetConnection()
    	{
			return new SqlConnection(CONNECTION_STRING);
    	}

    	protected override void CreateTable()
        {
            ExecuteSql(
				"CREATE TABLE " + TableName + "( " +
                "change_number INTEGER NOT NULL, " +
                "delta_set VARCHAR(10) NOT NULL, " +
                "start_dt DATETIME NOT NULL, " +
                "complete_dt DATETIME NULL, " +
                "applied_by VARCHAR(100) NOT NULL, " +
                "description VARCHAR(500) NOT NULL )");
            ExecuteSql(
				"ALTER TABLE " + TableName +
                " ADD CONSTRAINT Pkchangelog  PRIMARY KEY (change_number, delta_set)");
        }

        protected override void InsertRowIntoTable(int i)
        {
			ExecuteSql("INSERT INTO " + TableName
                       + " (change_number, delta_set, start_dt, complete_dt, applied_by, description) VALUES ( "
                       + i + ", '" + DELTA_SET
                       + "', getdate(), getdate(), user_name(), 'Unit test')");
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