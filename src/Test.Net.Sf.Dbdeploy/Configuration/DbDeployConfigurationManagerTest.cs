namespace Net.Sf.Dbdeploy.Configuration
{
    using System;
    using System.Linq;
    using System.Text;

    using NUnit.Framework;

    using Net.Sf.Dbdeploy.Database;

    /// <summary>
    /// Unit tests for XML configuration.
    /// </summary>
    [TestFixture]
    public class DbDeployConfigurationManagerTest
    {
        /// <summary>
        /// The configuration manager to test.
        /// </summary>
        private DbDeployConfigurationManager configurationManager;

        /// <summary>
        /// Sets up the dependencies before each test.
        /// </summary>
        [SetUp]
        protected void SetUp()
        {
            this.configurationManager = new DbDeployConfigurationManager();
        }

        /// <summary>
        /// Tests the correct parse of XML.
        /// </summary>
        [Test]
        public void ShouldParseXmlIntoDeployments()
        {
            var config = this.configurationManager.ReadConfiguration(@"Mocks\DbDeploy.config.xml");

            Assert.IsNotNull(config.Deployments, "Deployments should not be null.");
            Assert.Greater(config.Deployments.Count, 0, "There should be some deployment items.");

            // Test full config.
            var deployment = config.Deployments.First();
            Assert.AreEqual("mysql", deployment.Dbms, "Dbms is incorrect.");
            Assert.AreEqual(@"Server=.\;Initial Catalog=DBDEPLOY;User Id=DBDeployUser;Password=Password01", deployment.ConnectionString, "ConnectionString is incorrect.");
            Assert.IsNotNull(deployment.ScriptDirectory, "ScriptDirectory is not set.");
            Assert.AreEqual(@"C:\Database\Scripts\Versions", deployment.ScriptDirectory.FullName, "ScriptDirectory is incorrect.");
            Assert.IsNotNull(deployment.OutputFile, "OutputFile is not set.");
            Assert.AreEqual(@"C:\Database\Scripts\Output\dbdeploy.sql", deployment.OutputFile.FullName, "OutputFile is incorrect.");
            Assert.AreEqual("InstallLog", deployment.ChangeLogTableName, "ChangeLogTableName is incorrect.");
            Assert.IsFalse(deployment.AutoCreateChangeLogTable, "AutoCreateChangeLogTable is incorrect.");
            Assert.IsTrue(deployment.ForceUpdate, "ForceUpdate is incorrect.");
            Assert.IsTrue(deployment.UseSqlCmd, "UseSqlCmd is incorrect.");
            Assert.IsNotNull(deployment.LastChangeToApply, "LastChangeToApply is not set.");
            Assert.AreEqual("v1.1/4", deployment.LastChangeToApply.UniqueKey, "LastChangeToApply is incorrect.");
            Assert.AreEqual(Encoding.UTF32, deployment.Encoding, "Encoding is incorrect.");
            Assert.IsNotNull(deployment.TemplateDirectory, "TemplateDirectory is not set.");
            Assert.AreEqual(@"C:\Database\Templates", deployment.TemplateDirectory.FullName, "TemplateDirectory is incorrect.");
            Assert.AreEqual(@";", deployment.Delimiter, "Delimiter is incorrect.");
            Assert.IsInstanceOfType(typeof(RowDelimiter), deployment.DelimiterType, "DelimiterType is incorrect.");
            Assert.AreEqual(LineEnding.Lf, deployment.LineEnding, "LineEnding is incorrect.");

            // Partial config.
            deployment = config.Deployments.Last();
            Assert.AreEqual(DbDeployDefaults.Dbms, deployment.Dbms, "Dbms should be default.");
            Assert.AreEqual(@"Server=.\SQLEXPRESS;Initial Catalog=DBDEPLOY;User Id=DBDeployUser;Password=Password01", deployment.ConnectionString, "ConnectionString is incorrect.");
            Assert.IsNotNull(deployment.ScriptDirectory, "ScriptDirectory is not set.");
            Assert.AreEqual(@"C:\Database\Scripts\Versioning", deployment.ScriptDirectory.FullName, "ScriptDirectory is incorrect.");
            Assert.AreEqual(DbDeployDefaults.OutputFile, deployment.OutputFile, "OutputFile should be default.");
            Assert.AreEqual(DbDeployDefaults.ChangeLogTableName, deployment.ChangeLogTableName, "ChangeLogTableName should be default.");
            Assert.AreEqual(DbDeployDefaults.AutoCreateChangeLogTable, deployment.AutoCreateChangeLogTable, "AutoCreateChangeLogTable should be default.");
            Assert.AreEqual(DbDeployDefaults.ForceUpdate, deployment.ForceUpdate, "ForceUpdate should be default.");
            Assert.IsTrue(deployment.UseSqlCmd, "UseSqlCmd should be true.");
            Assert.AreEqual(DbDeployDefaults.LastChangeToApply, deployment.LastChangeToApply, "LastChange should be default..");
            Assert.AreEqual(DbDeployDefaults.Encoding, deployment.Encoding, "Encoding should be set to default.");
            Assert.AreEqual(DbDeployDefaults.TemplateDirectory, deployment.TemplateDirectory, "TemplateDirectory should be default..");
            Assert.AreEqual(DbDeployDefaults.Delimiter, deployment.Delimiter, "Delimiter should be default..");
            Assert.IsInstanceOfType(typeof(NormalDelimiter), deployment.DelimiterType, "DelimiterType is not default.");
            Assert.AreEqual(DbDeployDefaults.LineEnding, deployment.LineEnding, "LineEnding should default.");

            Assert.AreEqual(2, config.Deployments.Count, "There should be two deployments.");
        }
    }
}