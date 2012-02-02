using System.IO;
using Dbdeploy.Powershell;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy
{
    [TestFixture]
    public class TextXmlConfiguration
    {
        [Test]
        public void TestCorrectParseOfXml()
        {
            var text =
                @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<configuration>
  <appSettings>
    <add key=""db.connection"" value=""Server=localhost;Initial Catalog=DBDEPLOY;User Id=DBDeployUser;Password=password""/>
    <add key=""db.type"" value=""mssql""/>
    <add key=""db.tableName"" value=""changelog""/>
  </appSettings>
</configuration>";

            
            var config = new XmlConfiguration(new StringReader(text));

            Assert.AreEqual("mssql", config.DbType);
            Assert.AreEqual("Server=localhost;Initial Catalog=DBDEPLOY;User Id=DBDeployUser;Password=password", config.DbConnectionString);
            Assert.AreEqual("changelog", config.TableName);
        }
    }
}