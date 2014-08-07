using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using Net.Sf.Dbdeploy.Configuration;
using Net.Sf.Dbdeploy.Utils;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy
{
    [TestFixture]
    public class DbDeployerTest : DatabaseTestBase
    {
        public DbDeployerTest()
            : base(ConfigurationManager.AppSettings["ConnString"])
        {
        }

        [SetUp]
        public void Setup()
        {
            // Remove any existing changelog and customers test table.
            EnsureTableDoesNotExist(ChangeLogTableName);
            EnsureTableDoesNotExist("Product");
            EnsureTableDoesNotExist("Customer");
        }

        [Test]
        public void DbDebployCanExecuteBothEmbeddedScriptsAndScriptsInDirectories()
        {
            using (var tw = File.CreateText("database_" + DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss") + ".txt"))
            {
                var dbDeployer = new DbDeployer();
                var config = new DbDeployConfig
                    {
                        ConnectionString = ConnectionString,
                        Dbms = SupportedDbms.MSSQL,
                        Delimiter = "GO",
                        ScriptDirectory = new DirectoryInfo(@"Mocks\Versioned\2.0.0.0"),
                        ScriptAssembly = AssemblieWithEmbeddedScripts(),
                        DelimiterType = Parser.ParseDelimiterType("row"),
                        UseSqlCmd = false
                    };
                dbDeployer.Execute(config, tw);
            }

            AssertTableExists(ChangeLogTableName);
            AssertTableExists("Product");
            AssertTableExists("Customer");
        }

        [Test]
        public void DbDebployCanExecuteEmbeddedScripts()
        {
            using (var tw = File.CreateText("database_" + DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss") + ".txt"))
            {
                var dbDeployer = new DbDeployer();
                var config = new DbDeployConfig
                {
                    ConnectionString = ConnectionString,
                    Dbms = SupportedDbms.MSSQL,
                    Delimiter = "GO",
                    ScriptDirectory = null,
                    ScriptAssembly = AssemblieWithEmbeddedScripts(),
                    DelimiterType = Parser.ParseDelimiterType("row"),
                    UseSqlCmd = false
                };
                dbDeployer.Execute(config, tw);
            }

            AssertTableExists(ChangeLogTableName);
            AssertTableExists("Customer");
        }

        [Test]
        public void DbDebployCanExecuteDirectoryScripts()
        {
            using (var tw = File.CreateText("database_" + DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss") + ".txt"))
            {
                var dbDeployer = new DbDeployer();
                var config = new DbDeployConfig
                {
                    ConnectionString = ConnectionString,
                    Dbms = SupportedDbms.MSSQL,
                    Delimiter = "GO",
                    ScriptDirectory = new DirectoryInfo(@"Mocks\Versioned\2.0.0.0"),
                    DelimiterType = Parser.ParseDelimiterType("row"),
                    UseSqlCmd = false
                };
                dbDeployer.Execute(config, tw);
            }

            AssertTableExists(ChangeLogTableName);
            AssertTableExists("Product");
        }

        private Assembly AssemblieWithEmbeddedScripts()
        {
            var fileInfo = new FileInfo("Test.Net.Sf.DbDeploy.EmbeddedScripts.dll");
            return Assembly.LoadFile(fileInfo.FullName);
        }
    }
}