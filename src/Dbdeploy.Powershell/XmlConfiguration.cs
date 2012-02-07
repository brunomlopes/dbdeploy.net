using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Net.Sf.Dbdeploy.Configuration;

namespace Dbdeploy.Powershell
{
    public class XmlConfiguration : IConfiguration
    {
        private Dictionary<string, string> keys;

        public XmlConfiguration(string xmlFilePath)
        {
            using(var file = new StreamReader(File.OpenRead(xmlFilePath)))
            {
                LoadXmlFile(file);
            }
        }

        public XmlConfiguration(TextReader xmlFile)
        {
            LoadXmlFile(xmlFile);
        }

        private void LoadXmlFile(TextReader xmlFile)
        {
            XDocument doc = XDocument.Load(xmlFile);
            this.keys = doc.XPathSelectElements("/configuration/appSettings/add")
                .ToDictionary(n => n.Attribute("key").Value,
                              n => n.Attribute("value").Value);

            DbConnectionString = ValueOrException("db.connection", "Connection string");
            DbType = ValueOrException("db.type", "Database type");
            TableName = ValueOrException("db.tableName", "Table name for change log");
        }

        private string ValueOrException(string key, string message)
        {
            if (!keys.ContainsKey(key))
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Missing key '{0}' ({1})", key, message));
            }
            return keys[key];
        }

        public string DbConnectionString { get; private set; }
        public string DbType { get; private set; }
        public string TableName { get; private set; }
    }
}