namespace Net.Sf.Dbdeploy.Database
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;

    using NUnit.Framework;

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

        /// <summary>
        /// Tests that <see cref="DatabaseSchemaVersionManager" /> can create a change log table under a specified schema.
        /// </summary>
        [Test]
        public void TestShouldHandleCreatingChangeLogTableWithSchema()
        {
            this.EnsureTableDoesNotExist("log.Installs");

            var factory = new DbmsFactory(this.Dbms, this.ConnectionString);
            var executer = new QueryExecuter(factory);
            var databaseSchemaManager = new DatabaseSchemaVersionManager(executer, factory.CreateDbmsSyntax(), "log.Installs", true);

            // Table should be created when attempted now; if table does not exist.
            databaseSchemaManager.GetAppliedChanges();

            this.AssertTableExists("log.Installs");
        }

        /// <summary>
        /// Ensures the table does not exist.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        protected override void EnsureTableDoesNotExist(string tableName)
        {
            var syntax = new MsSqlDbmsSyntax();
            var tableInfo = syntax.GetTableInfo(tableName);
            this.ExecuteSql(string.Format(
                CultureInfo.InvariantCulture,
@"IF (EXISTS (SELECT * 
    FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = '{0}' 
    AND  TABLE_NAME = '{1}'))
BEGIN
    DROP Table {0}.{1}
END", 
                tableInfo.Schema, tableInfo.TableName));
        }

        protected override IDbConnection GetConnection()
        {
            return new SqlConnection(CONNECTION_STRING);
        }

        protected override void InsertRowIntoTable(int i)
        {
            this.ExecuteSql("INSERT INTO " + TableName
                       + " (Folder, ScriptNumber, StartDate, CompleteDate, AppliedBy, ScriptName, ScriptStatus, ScriptOutput) VALUES ( "
                       + "'" + FOLDER + "', " + i
                       + ", getdate(), getdate(), user_name(), 'Unit test', 1, '')");
        }
    }
}