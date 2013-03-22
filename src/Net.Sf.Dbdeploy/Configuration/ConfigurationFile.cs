namespace Net.Sf.Dbdeploy.Configuration
{
    using System.Configuration;

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