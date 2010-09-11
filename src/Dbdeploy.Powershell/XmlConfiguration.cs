using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using Net.Sf.Dbdeploy.Configuration;
using Enumerable = System.Linq.Enumerable;

namespace Dbdeploy.Powershell.Commands
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
            this.keys = Enumerable.ToDictionary(doc.XPathSelectElements("/configuration/appSettings/add"), n => n.Attribute("key").Value, n => n.Attribute("value").Value);

            DbConnectionString = ValueOrException("db.connection", "Connection string");
            DbType = ValueOrException("db.type", "Database type");
            DbDeltaSet = ValueOrException("db.deltaSet", "Delta set name");
            TableName = ValueOrException("db.tableName", "Table name for change log");
            UseTransaction = ValueOrDefault("db.useTransaction", false);
        }

        private bool ValueOrDefault(string key, bool defaultValue)
        {
            return keys.ContainsKey(key) ? keys[key].Trim().ToLowerInvariant() == "true" : defaultValue;
        }

        private string ValueOrException(string key, string message)
        {
            if (!keys.ContainsKey(key))
            {
                throw new InvalidOperationException(string.Format("Missing key '{0}' ({1})", key, message));
            }
            return keys[key];
        }

        public string DbConnectionString { get; private set; }
        public string DbType { get; private set; }
        public string DbDeltaSet { get; private set; }
        public bool UseTransaction { get; private set; }
        public int? CurrentDbVersion { get; private set; }
        public string TableName { get; private set; }
    }
}