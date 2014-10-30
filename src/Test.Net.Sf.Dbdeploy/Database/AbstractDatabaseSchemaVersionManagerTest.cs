using System;

namespace Net.Sf.Dbdeploy.Database
{
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Linq;

    using Exceptions;

    using NUnit.Framework;

    [TestFixture]
    public abstract class AbstractDatabaseSchemaVersionManagerTest
    {
        public const string TableName = "ChangeLog";
        protected DatabaseSchemaVersionManager databaseSchemaVersion;
        protected IDbmsSyntax syntax;
        private readonly string[] CHANGELOG_TABLE_DOES_NOT_EXIST_MESSAGES = new[] 
        {
            "No table found with name 'ChangeLog'.",
        };

        [SetUp]
        protected virtual void SetUp()
        {
            var factory = new DbmsFactory(Dbms, ConnectionString);
            var executer = new QueryExecuter(factory);

            this.syntax = factory.CreateDbmsSyntax();
            databaseSchemaVersion = new DatabaseSchemaVersionManager(executer, this.syntax, TableName);
        }

        public virtual void TestCanRetrieveSchemaVersionFromDatabase()
        {
            this.EnsureTableDoesNotExist();
            CreateTable();
            InsertRowIntoTable(5);

            var appliedChanges = new List<ChangeEntry>(databaseSchemaVersion.GetAppliedChanges());
            Assert.AreEqual(1, appliedChanges.Count);
            Assert.AreEqual("Scripts/5", appliedChanges[0].UniqueKey);
        }

        public virtual void TestReturnsNoAppliedChangesWhenDatabaseTableDoesNotExist()
        {
            this.EnsureTableDoesNotExist();

            Assert.IsEmpty(databaseSchemaVersion.GetAppliedChanges().ToArray());
        }

        public virtual void TestThrowsWhenDatabaseTableDoesNotExist()
        {
            this.EnsureTableDoesNotExist();

            try
            {
                databaseSchemaVersion.GetAppliedChanges();
                Assert.Fail("expected exception");
            }
            catch (ChangelogTableDoesNotExistException ex)
            {
                // Allow exception messages in different languages.
                // Kind of complicated, but with this way you see [expected] vs. [actual] on test failure.

                string expected = ChangelogTableDoesNotExistMessages()[0];

                for (int i = 0; i < ChangelogTableDoesNotExistMessages().Length; i++)
                {
                    if (ChangelogTableDoesNotExistMessages()[i].Equals(ex.Message))
                    {
                        expected = ChangelogTableDoesNotExistMessages()[i];
                    }
                }

                Assert.AreEqual(expected, ex.Message);
            }
        }

        /// <summary>
        /// Tests that <see cref="DatabaseSchemaVersionManager" /> will create the change log table when specified.
        /// </summary>
        public virtual void TestShouldCreateChangeLogTableWhenToldToDoSo()
        {
            this.EnsureTableDoesNotExist();

            // Create change log table
            CreateTable();

            this.AssertTableExists("ChangeLog");
        }

        /// <summary>
        /// Asserts the table exists.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        public virtual void AssertTableExists(string tableName)
        {
            var schema = this.ExecuteScalar<string>(this.syntax.TableExists(tableName));

            Assert.IsNotNull(schema, string.Format("{0} table was not created.", tableName));
            Assert.IsNotEmpty(schema, string.Format("{0} table was not created.", tableName));
        }

        /// <summary>
        /// Asserts that the table does not exist
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        public virtual void AssertTableDoesNotExist(string tableName)
        {
            var schema = this.ExecuteScalar<string>(this.syntax.TableExists(tableName));

            Assert.IsNull(schema, string.Format("{0} table was created.", tableName));
        }

        public virtual void TestShouldReturnEmptySetWhenTableHasNoRows()
        {
            this.EnsureTableDoesNotExist();
            CreateTable();

            Assert.AreEqual(0, databaseSchemaVersion.GetAppliedChanges().Count);
        }

        /// <summary>
        /// Ensures the table does not exist.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        protected virtual void EnsureTableDoesNotExist(string tableName)
        {
            ExecuteSql("DROP TABLE " + TableName);
        }

        protected void ExecuteSql(string sql)
        {
            using (IDbConnection connection = GetConnection())
            {
                try
                {
                    connection.Open();
                    IDbCommand command = connection.CreateCommand();
                    command.CommandText = sql;
                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    
                }
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

        /// <summary>
        /// Creats the change log table.
        /// </summary>
        protected virtual void CreateTable()
        {
            if (!this.databaseSchemaVersion.ChangeLogTableExists())
            {
                var script = this.syntax.CreateChangeLogTableSqlScript(TableName);
                script = script.Replace(";", string.Empty);
                ExecuteSql(script);
            };
        }

        /// <summary>
        /// Ensures the change log table does not exist.
        /// </summary>
        public void EnsureTableDoesNotExist()
        {
            this.EnsureTableDoesNotExist(TableName);
        }

        protected string[] ChangelogTableDoesNotExistMessages()
        {
            return CHANGELOG_TABLE_DOES_NOT_EXIST_MESSAGES;
        }

        protected abstract string ConnectionString { get; }
        protected abstract string Folder { get; }
        protected abstract string Dbms { get; }
        protected abstract IDbConnection GetConnection();
        protected abstract void InsertRowIntoTable(int i);
        public abstract void ShouldNotThrowExceptionIfAllPreviousScriptsAreCompleted();
    }
}