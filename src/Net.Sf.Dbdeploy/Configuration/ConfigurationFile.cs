using System;
using System.Configuration;

namespace Net.Sf.Dbdeploy.Configuration
{
    public class ConfigurationFile : IConfiguration
    {
        public string DbConnectionString
        {
            get { return ConfigurationManager.AppSettings["db.connection"]; }
        }

        public string DbType
        {
            get { return ConfigurationManager.AppSettings["db.type"]; }
        }

        public string TableName
        {
            get { return ConfigurationManager.AppSettings["db.tableName"]; }
        }
    }
}