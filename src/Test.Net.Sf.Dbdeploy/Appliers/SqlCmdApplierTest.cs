namespace Net.Sf.Dbdeploy.Appliers
{
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.IO;
    using System.Text;

    using Net.Sf.Dbdeploy.Database;
    using Net.Sf.Dbdeploy.Exceptions;
    using Net.Sf.Dbdeploy.Scripts;

    using NUnit.Framework;

    /// <summary>
    /// Tests for <see cref="SqlCmdApplier"/> class.
    /// </summary>
    [TestFixture]
    public class SqlCmdApplierTest
    {
        /// <summary>
        /// The table name for the change tracking table.
        /// </summary>
        public const string ChangeLogTableName = "ChangeLog";

        /// <summary>
        /// The database management system type.
        /// </summary>
        private const string Dbms = "mssql";

        /// <summary>
        /// The connection string for the test database.
        /// </summary>
        private static readonly string ConnectionString = ConfigurationManager.AppSettings["ConnString"];

        /// <summary>
        /// Target of the test.
        /// </summary>
        private SqlCmdApplier sqlCmdApplier;

        /// <summary>
        /// The directory scanner for finding script files.
        /// </summary>
        private DirectoryScanner directoryScanner;

        /// <summary>
        /// Sets up the dependencies before each test.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            var dbmsFactory = new DbmsFactory(Dbms, ConnectionString);
            var queryExecuter = new QueryExecuter(dbmsFactory);

            var schemaVersionManager = new DatabaseSchemaVersionManager(queryExecuter, dbmsFactory.CreateDbmsSyntax(), ChangeLogTableName, true);
            this.sqlCmdApplier = new SqlCmdApplier(ConnectionString, schemaVersionManager, System.Console.Out);
            this.directoryScanner = new DirectoryScanner(System.Console.Out, Encoding.UTF8);

            // Remove any existing changelog and customers test table.
            this.EnsureTableDoesNotExist(ChangeLogTableName);
            this.EnsureTableDoesNotExist("Customer");

            // Create the change log table.
            schemaVersionManager.VerifyChangeLogTableExists(true);
        }

        /// <summary>
        /// Tests that <see cref="SqlCmdApplier" /> can apply scripts written for SQLCMD.
        /// </summary>
        [Test]
        public void ShouldApplySqlCmdModeScripts()
        {
            var changeScripts = this.directoryScanner.GetChangeScriptsForDirectory(new DirectoryInfo(@"Mocks\Versioned\v2.0.10.0"));
            this.sqlCmdApplier.Apply(changeScripts);

            this.AssertTableExists(ChangeLogTableName);
            this.AssertTableExists("Customer");

            var changeCount = this.ExecuteScalar<int>("SELECT COUNT(*) FROM {0}", ChangeLogTableName);
            Assert.AreEqual(changeScripts.Count, changeCount, "Not all change scripts where applied.");
        }

        /// <summary>
        /// Tests that <see cref="SqlCmdApplier" /> throws an exception and stops execution when there is a problem.
        /// </summary>
        [Test]
        public void ShouldThrowExceptionOnScriptFailure()
        {
            // Duplicate the first script to cause a failure.
            var changeScripts = this.directoryScanner.GetChangeScriptsForDirectory(new DirectoryInfo(@"Mocks\Failures"));

            try
            {
                this.sqlCmdApplier.Apply(changeScripts);
                Assert.Fail("Apply did not thrown and error.");
            }
            catch (DbDeployException)
            {
            }

            var changeCount = this.ExecuteScalar<int>("SELECT COUNT(*) FROM {0}", ChangeLogTableName);
            Assert.AreEqual(2, changeCount, "Only two scripts should have run.");
        }

        /// <summary>
        /// Asserts the table does exist.
        /// </summary>
        /// <param name="name">The name of the table.</param>
        private void AssertTableExists(string name)
        {
            var syntax = new MsSqlDbmsSyntax();
            var schema = this.ExecuteScalar<string>(syntax.TableExists(name));

            Assert.IsNotEmpty(schema, "'{0}' table was not created.", name);            
        }

        /// <summary>
        /// Drops the table to ensure it does not exist.
        /// </summary>
        /// <param name="name">The name.</param>
        private void EnsureTableDoesNotExist(string name)
        {
            this.ExecuteSql(string.Format(CultureInfo.InvariantCulture, 
@"IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND TYPE IN (N'U'))
DROP TABLE [dbo].[{0}]", name));
        }

        /// <summary>
        /// Executes the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        private void ExecuteSql(string sql)
        {
            using (var connection = this.GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = sql;
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Executes a query returning a scalar value.
        /// </summary>
        /// <typeparam name="T">Scalar value to be returned.</typeparam>
        /// <param name="sql">The SQL.</param>
        /// <param name="args">The arguments to format into the script.</param>
        /// <returns>
        /// Scalar value from query.
        /// </returns>
        private T ExecuteScalar<T>(string sql, params object[] args)
        {
            T result;
            using (var connection = this.GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = string.Format(CultureInfo.InvariantCulture, sql, args);
                result = (T)command.ExecuteScalar();
            }

            return result;
        }

        /// <summary>
        /// Gets a database connection.
        /// </summary>
        /// <returns>Database connection.</returns>
        private IDbConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }
    }
}
