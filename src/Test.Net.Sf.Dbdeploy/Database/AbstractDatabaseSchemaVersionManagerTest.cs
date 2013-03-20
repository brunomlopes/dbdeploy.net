using System;
using System.Collections.Generic;
using System.Data;
using Net.Sf.Dbdeploy.Exceptions;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Database
{
    using System.Globalization;

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

            databaseSchemaVersion = new DatabaseSchemaVersionManager(executer, factory.CreateDbmsSyntax(), TableName, false);
        }

        public virtual void TestCanRetrieveSchemaVersionFromDatabase()
        {
            EnsureTableDoesNotExist();
            CreateTable();
            InsertRowIntoTable(5);

            var appliedChanges = new List<ChangeEntry>(databaseSchemaVersion.GetAppliedChanges());
            Assert.AreEqual(1, appliedChanges.Count);
            Assert.Contains(5, appliedChanges);
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

        /// <summary>
        /// Tests that <see cref="DatabaseSchemaVersionManager" /> will create the change log table when specified.
        /// </summary>
        public virtual void TestShouldCreateChangeLogTableWhenDoesNotExist()
        {
            this.databaseSchemaVersion.AutoCreateChangeLogTable = true;

            this.EnsureTableDoesNotExist();

            // Table should be created when attempted now; if table does not exist.
            databaseSchemaVersion.GetAppliedChanges();

            string schema = this.ExecuteScalar<string>(@"
SELECT table_schema 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME = 'ChangeLog'");

            Assert.IsNotEmpty(schema, "ChangeLog table was not created.");
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

        protected void ExecuteSql(string sql)
        {
            using (IDbConnection connection = GetConnection())
            {
                connection.Open();
                IDbCommand command = connection.CreateCommand();
                command.CommandText = sql;
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Executes a query returning a scalar value.
        /// </summary>
        /// <typeparam name="T">Scalar value to be returned.</typeparam>
        /// <param name="sql">The SQL.</param>
        /// <returns>Scalar value from query.</returns>
        protected T ExecuteScalar<T>(string sql, params object[] args)
        {
            T result = default(T);
            using (IDbConnection connection = GetConnection())
            {
                connection.Open();
                IDbCommand command = connection.CreateCommand();
                command.CommandText = string.Format(CultureInfo.InvariantCulture, sql, args);
                result = (T)command.ExecuteScalar();
            }

            return result;
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