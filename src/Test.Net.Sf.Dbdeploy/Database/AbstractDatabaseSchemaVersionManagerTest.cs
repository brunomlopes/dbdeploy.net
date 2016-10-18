namespace Net.Sf.Dbdeploy.Database
{
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Linq;

    using Net.Sf.Dbdeploy.Exceptions;

    using NUnit.Framework;

    [TestFixture]
    public abstract class AbstractDatabaseSchemaVersionManagerTest
    {
        public const string TableName = "ChangeLog";

        protected DatabaseSchemaVersionManager databaseSchemaVersion;

		protected IDbmsSyntax syntax;

        [SetUp]
        protected void SetUp()
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
        public virtual void TestShouldCreateChangeLogTableWhenToldToDoSo()
        {
            this.EnsureTableDoesNotExist();

            // Create change log table
            databaseSchemaVersion.CreateChangeLogTable();

            this.AssertTableExists("ChangeLog");
        }

        /// <summary>
        /// Asserts the table exists.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        public void AssertTableExists(string tableName)
        {
            var schema = this.ExecuteScalar<string>(this.syntax.TableExists(tableName));

            Assert.IsNotNull(schema, string.Format("{0} table was not created.", tableName));
            Assert.IsNotEmpty(schema, string.Format("{0} table was not created.", tableName));
        } 
        
        /// <summary>
        /// Asserts that the table does not exist
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        public void AssertTableDoesNotExist(string tableName)
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

        /// <summary>
        /// Creats the change log table.
        /// </summary>
        protected void CreateTable()
        {
            if (!this.databaseSchemaVersion.ChangeLogTableExists())
            {
                this.databaseSchemaVersion.CreateChangeLogTable();
            };
        }

        /// <summary>
        /// Ensures the change log table does not exist.
        /// </summary>
        public void EnsureTableDoesNotExist()
        {
            this.EnsureTableDoesNotExist(TableName);
        }

        protected abstract string ConnectionString { get; }
        protected abstract string Folder { get; }
        protected abstract string[] ChangelogTableDoesNotExistMessages { get; }
        protected abstract string Dbms { get; }
        protected abstract IDbConnection GetConnection();
        protected abstract void InsertRowIntoTable(int i);
    }
}