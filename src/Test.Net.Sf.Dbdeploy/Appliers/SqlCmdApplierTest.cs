using Net.Sf.Dbdeploy.Utils;

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
    public class SqlCmdApplierTest : DatabaseTestBase
    {
        public SqlCmdApplierTest()
            : base(ConfigurationManager.AppSettings["ConnString"])
        {
        }
        
        /// <summary>
        /// The database management system type.
        /// </summary>
        private const string Dbms = "mssql";
        
        /// <summary>
        /// Target of the test.
        /// </summary>
        private SqlCmdApplier sqlCmdApplier;


        /// <summary>
        /// Sets up the dependencies before each test.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            var dbmsFactory = new DbmsFactory(Dbms, ConnectionString);
            var queryExecuter = new QueryExecuter(dbmsFactory);
            var dbmsSyntax = dbmsFactory.CreateDbmsSyntax();

            var schemaVersionManager = new DatabaseSchemaVersionManager(queryExecuter, dbmsSyntax, ChangeLogTableName);
            this.sqlCmdApplier = new SqlCmdApplier(ConnectionString, schemaVersionManager, dbmsSyntax, ChangeLogTableName, System.Console.Out);
            

            // Remove any existing changelog and customers test table.
            this.EnsureTableDoesNotExist(ChangeLogTableName);
            this.EnsureTableDoesNotExist("Customer");
        }

        /// <summary>
        /// Tests that <see cref="SqlCmdApplier" /> can apply scripts written for SQLCMD.
        /// </summary>
        [Test]
        public void ShouldApplySqlCmdModeScripts()
        {
            var directoryScanner = new DirectoryScanner(System.Console.Out, Encoding.UTF8, new DirectoryInfo(@"Mocks\Versioned\v2.0.10.0"));

            var changeScripts = directoryScanner.GetChangeScripts();
            this.sqlCmdApplier.Apply(changeScripts, true);

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
            var directoryScanner = new DirectoryScanner(System.Console.Out, Encoding.UTF8,
                                                        new DirectoryInfo(@"Mocks\Failures"));

            // Duplicate the first script to cause a failure.
            var changeScripts = directoryScanner.GetChangeScripts();

            try
            {
                this.sqlCmdApplier.Apply(changeScripts, true);
                Assert.Fail("Apply did not thrown and error.");
            }
            catch (DbDeployException)
            {
            }

            var changeCount = this.ExecuteScalar<int>("SELECT COUNT(*) FROM {0}", ChangeLogTableName);
            Assert.AreEqual(2, changeCount, "Only two scripts should have run.");
        }

    }
}
