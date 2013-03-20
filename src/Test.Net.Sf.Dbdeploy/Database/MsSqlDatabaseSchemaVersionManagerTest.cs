using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Database
{
    [Category("MSSQL"), Category("DbIntegration")]
    public class MsSqlDatabaseSchemaVersionManagerTest : AbstractDatabaseSchemaVersionManagerTest
    {
        private static readonly string CONNECTION_STRING = ConfigurationManager.AppSettings["ConnString"];
        private const string FOLDER = "Scripts";

        private readonly string[] CHANGELOG_TABLE_DOES_NOT_EXIST_MESSAGES = new [] 
        {
            "No table found with name 'ChangeLog'.",
        };

		private const string DBMS = "mssql";

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

    	protected override void EnsureTableDoesNotExist()
        {
            ExecuteSql(string.Format(
                CultureInfo.InvariantCulture,
				"IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[{0}]') AND type in (N'U')) DROP TABLE [{0}]", 
                TableName));
        }

    	protected override IDbConnection GetConnection()
    	{
			return new SqlConnection(CONNECTION_STRING);
    	}

    	protected override void CreateTable()
        {
            ExecuteSql(
				"CREATE TABLE " + TableName + "( " +
                "ScriptNumber INTEGER NOT NULL, " +
                "Folder VARCHAR(256) NOT NULL, " +
                "StartDate DATETIME NOT NULL, " +
                "CompleteDate DATETIME NULL, " +
                "AppliedBy VARCHAR(100) NOT NULL, " +
                "FileName VARCHAR(500) NOT NULL )");
            ExecuteSql(
				"ALTER TABLE " + TableName +
                " ADD CONSTRAINT PK_ChangeLog  PRIMARY KEY (Folder, ScriptNumber)");
        }

        protected override void InsertRowIntoTable(int i)
        {
			ExecuteSql("INSERT INTO " + TableName
                       + " (ScriptNumber, Folder, StartDate, CompleteDate, AppliedBy, FileName) VALUES ( "
                       + i + ", '" + FOLDER
                       + "', getdate(), getdate(), user_name(), 'Unit test')");
        }

    	[Test]
    	public void ShouldNotThrowExceptionIfAllPreviousScriptsAreCompleted()
    	{
			EnsureTableDoesNotExist();
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