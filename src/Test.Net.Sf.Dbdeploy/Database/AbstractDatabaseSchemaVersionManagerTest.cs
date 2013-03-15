using System;
using System.Collections.Generic;
using System.Data;
using Net.Sf.Dbdeploy.Exceptions;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Database
{
    [TestFixture]
    public abstract class AbstractDatabaseSchemaVersionManagerTest
    {
        public const string TableName = "ChangeLog";

        protected DatabaseSchemaVersionManager databaseSchemaVersion;

        [SetUp]
        protected void SetUp()
        {
            var factory = new DbmsFactory(Dbms, ConnectionString);
            var executer = new QueryExecuter(factory);

            databaseSchemaVersion = new DatabaseSchemaVersionManager(executer, factory.CreateDbmsSyntax(), TableName);
        }

        public virtual void TestCanRetrieveSchemaVersionFromDatabase()
        {
            EnsureTableDoesNotExist();
            CreateTable();
            InsertRowIntoTable(5);

            List<int> appliedChangeNumbers = new List<int>(databaseSchemaVersion.GetAppliedChanges());
            Assert.AreEqual(1, appliedChangeNumbers.Count);
            Assert.Contains(5, appliedChangeNumbers);
        }

        public virtual void TestThrowsWhenDatabaseTableDoesNotExist()
        {
            EnsureTableDoesNotExist();

            try
            {
                databaseSchemaVersion.GetAppliedChanges();
                Assert.Fail("expected exception");
            }
            catch (ChangelogTableDoesNotExistException ex)
            {
                // Allow exception messages in different languages.
                // Kind of complicated, but with this way you see [expected] vs. [actual] on test failure.

                string expected = ChangelogTableDoesNotExistMessages[0];

                for (int i = 0; i < ChangelogTableDoesNotExistMessages.Length; i++)
                {
                    if (ChangelogTableDoesNotExistMessages[i].Equals(ex.Message))
                    {
                        expected = ChangelogTableDoesNotExistMessages[i];
                    }
                }

                Assert.AreEqual(expected, ex.Message);
            }
        }

        public virtual void TestShouldReturnEmptySetWhenTableHasNoRows()
        {
            EnsureTableDoesNotExist();
            CreateTable();

            Assert.AreEqual(0, databaseSchemaVersion.GetAppliedChanges().Count);
        }

        protected virtual void EnsureTableDoesNotExist()
        {
            ExecuteSql("DROP TABLE " + TableName);
        }

        protected void ExecuteSql(String sql)
        {
            using (IDbConnection connection = GetConnection())
            {
                connection.Open();
                IDbCommand command = connection.CreateCommand();
                command.CommandText = sql;
                command.ExecuteNonQuery();
            }
        }

        protected abstract string ConnectionString { get; }
        protected abstract string Folder { get; }
        protected abstract string[] ChangelogTableDoesNotExistMessages { get; }
        protected abstract string Dbms { get; }
        protected abstract IDbConnection GetConnection();
        protected abstract void CreateTable();
        protected abstract void InsertRowIntoTable(int i);
    }
}