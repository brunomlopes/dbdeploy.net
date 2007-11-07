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

        public string DbDeltaSet
        {
            get { return ConfigurationManager.AppSettings["db.deltaSet"]; }
        }

        public int? CurrentDbVersion
        {
            get
            {
                string versionSetting = ConfigurationManager.AppSettings["db.currentversion"];
                int version;
                Int32.TryParse(versionSetting, out version);
                if (version == 0) return null;
                return version;
            }
        }
    }
}