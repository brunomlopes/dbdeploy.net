using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using Net.Sf.Dbdeploy.Exceptions;
using Net.Sf.Dbdeploy.Scripts;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Database
{
    [TestFixture]
    public abstract class AbstractDatabaseSchemaVersionManagerTest
    {
        protected DatabaseSchemaVersionManager databaseSchemaVersion;

        [SetUp]
        protected void SetUp()
        {
            databaseSchemaVersion =
                new DatabaseSchemaVersionManager(new DbmsFactory("mssql", ConnectionString), DeltaSet);
        }

        public virtual void TestCanRetrieveSchemaVersionFromDatabase()
        {
            EnsureTableDoesNotExist();
            CreateTable();
            InsertRowIntoTable(5);

            List<int> appliedChangeNumbers = databaseSchemaVersion.GetAppliedChangeNumbers();
            Assert.AreEqual(1, appliedChangeNumbers.Count);
            Assert.Contains(5, appliedChangeNumbers);
        }

        public virtual void TestThrowsWhenDatabaseTableDoesNotExist()
        {
            EnsureTableDoesNotExist();

            try
            {
                databaseSchemaVersion.GetAppliedChangeNumbers();
                Assert.Fail("expected exception");
            }
            catch (SchemaVersionTrackingException ex)
            {
                Assert.AreEqual(ChangelogTableDoesNotExistMessage, ex.Message);
            }
        }

        public virtual void TestShouldReturnEmptySetWhenTableHasNoRows()
        {
            EnsureTableDoesNotExist();
            CreateTable();

            Assert.AreEqual(0, databaseSchemaVersion.GetAppliedChangeNumbers().Count);
        }

        public virtual void TestCanRetrieveDeltaFragmentHeaderSql()
        {
            ChangeScript script = new ChangeScript(3, "description");
            Assert.AreEqual(
                "--------------- Fragment begins: #3 ---------------\nINSERT INTO changelog (change_number, delta_set, start_dt, applied_by, description) VALUES (3, 'All', getdate(), user_name(), 'description')\nGO",
                databaseSchemaVersion.GenerateDoDeltaFragmentHeader(script));
        }

        public virtual void TestCanRetrieveDeltaFragmentFooterSql()
        {
            ChangeScript script = new ChangeScript(3, "description");
            Assert.AreEqual(
                "UPDATE changelog SET complete_dt = getdate() WHERE change_number = 3 AND delta_set = 'All'\nGO\n--------------- Fragment ends: #3 ---------------\n",
                databaseSchemaVersion.GenerateDoDeltaFragmentFooter(script));
        }

        protected virtual void EnsureTableDoesNotExist()
        {
            ExecuteSql("DROP TABLE " + DatabaseSchemaVersionManager.TABLE_NAME);
        }

        protected void ExecuteSql(String sql)
        {
            using (DbConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                DbCommand command = connection.CreateCommand();
                command.CommandText = sql;
                command.ExecuteNonQuery();
            }
        }

        protected abstract string ConnectionString { get; }
        protected abstract string DeltaSet { get; }
        protected abstract string ChangelogTableDoesNotExistMessage { get; }
        protected abstract void CreateTable();
        protected abstract void InsertRowIntoTable(int i);
    }
}