namespace Net.Sf.Dbdeploy.Database.SqlCmd
{
    using System.IO;

    using NUnit.Framework;

    /// <summary>
    /// Tests for <see cref="ConnectionStringParser"/> class.
    /// </summary>
    [TestFixture]
    public class ConnectionStringParserTest
    {
        /// <summary>
        /// Tests that <see cref="ConnectionStringParser" /> can parse a standard security string.
        /// </summary>
        [Test]
        public void ShouldParseStandardSecurity()
        {
            var connectionStringInfo = ConnectionStringParser.Parse(@"Server=myServerName\myInstanceName;Database=myDataBase;User Id=myUsername;Password=myPassword;");
            Assert.AreEqual(@"myServerName\myInstanceName", connectionStringInfo.Server, "Server name is incorrect.");
            Assert.AreEqual("myDataBase", connectionStringInfo.Database, "Database name is incorrect.");
            Assert.AreEqual("myUsername", connectionStringInfo.UserId, "User ID is incorrect.");
            Assert.AreEqual("myPassword", connectionStringInfo.Password, "Password is incorrect.");
            Assert.IsFalse(connectionStringInfo.TrustedConnection, "TrustedConnection should not be set.");
        }

        /// <summary>
        /// Tests that <see cref="ConnectionStringParser" /> can parse a trusted connection string.
        /// </summary>
        [Test]
        public void ShouldParseTrustedConnection()
        {
            var connectionStringInfo = ConnectionStringParser.Parse("Server=myServerAddress;Database=myDataBase;Trusted_Connection=True;");
            Assert.AreEqual("myServerAddress", connectionStringInfo.Server, "Server name is incorrect.");
            Assert.AreEqual("myDataBase", connectionStringInfo.Database, "Database name is incorrect.");
            Assert.IsNull(connectionStringInfo.UserId, "User ID should not be set.");
            Assert.IsNull(connectionStringInfo.Password, "Password should not be set.");
            Assert.IsTrue(connectionStringInfo.TrustedConnection, "TrustedConnection should be true.");
        }

        /// <summary>
        /// Tests that <see cref="ConnectionStringParser" /> can parse a data source connection string.
        /// </summary>
        [Test]
        public void ShouldParseDataSource()
        {
            var connectionStringInfo = ConnectionStringParser.Parse(@"Data Source=myServerAddress;Initial Catalog=myDataBase;Integrated Security=SSPI;User ID=myDomain\myUsername;Password=myPassword;");
            Assert.AreEqual(@"myServerAddress", connectionStringInfo.Server, "Server name is incorrect.");
            Assert.AreEqual("myDataBase", connectionStringInfo.Database, "Database name is incorrect.");
            Assert.AreEqual(@"myDomain\myUsername", connectionStringInfo.UserId, "User ID is incorrect.");
            Assert.AreEqual("myPassword", connectionStringInfo.Password, "Password is incorrect.");
            Assert.IsFalse(connectionStringInfo.TrustedConnection, "TrustedConnection should not be set.");
        }
    }
}