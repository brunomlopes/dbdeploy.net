using System.Configuration;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Database
{
    public class MsSqlDatabaseSchemaVersionManagerTest : AbstractDatabaseSchemaVersionManagerTest
    {
        private static readonly string CONNECTION_STRING = ConfigurationManager.AppSettings["ConnString"];
        private static readonly string DELTA_SET = "All";
        private static readonly string CHANGELOG_TABLE_DOES_NOT_EXIST_MESSAGE =
            "Could not retrieve change log from database because: Invalid object name 'dbo.changelog'.";

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

        protected override void EnsureTableDoesNotExist()
        {
            ExecuteSql(string.Format(
                "IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[{0}]') AND type in (N'U')) DROP TABLE [{0}]", DatabaseSchemaVersionManager.TABLE_NAME));
        }

        protected override void CreateTable()
        {
            ExecuteSql(
                "CREATE TABLE " + DatabaseSchemaVersionManager.TABLE_NAME + "( " +
                "change_number INTEGER NOT NULL, " +
                "delta_set VARCHAR(10) NOT NULL, " +
                "start_dt DATETIME NOT NULL, " +
                "complete_dt DATETIME NULL, " +
                "applied_by VARCHAR(100) NOT NULL, " +
                "description VARCHAR(500) NOT NULL )");
            ExecuteSql(
                "ALTER TABLE " + DatabaseSchemaVersionManager.TABLE_NAME +
                " ADD CONSTRAINT Pkchangelog  PRIMARY KEY (change_number, delta_set)");
        }

        protected override void InsertRowIntoTable(int i)
        {
            ExecuteSql("INSERT INTO " + DatabaseSchemaVersionManager.TABLE_NAME
                       + " (change_number, delta_set, start_dt, complete_dt, applied_by, description) VALUES ( "
                       + i + ", '" + DELTA_SET
                       + "', getdate(), getdate(), user_name(), 'Unit test')");
        }

        [Test]
        public override void TestCanRetrieveDeltaFragmentFooterSql()
        {
            base.TestCanRetrieveDeltaFragmentFooterSql();
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
            base.TestCanRetrieveDeltaFragmentHeaderSql();
        }
    }
}