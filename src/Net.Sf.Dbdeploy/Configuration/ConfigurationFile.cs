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

    	public bool UseTransaction
    	{
			get
			{
				string useTransactionSetting = ConfigurationManager.AppSettings["db.usetransaction"];
				bool useTransaction;
				bool.TryParse(useTransactionSetting, out useTransaction);

				return useTransaction;
			}
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

    	public string TableName
    	{
			get { return ConfigurationManager.AppSettings["db.tableName"]; }
    	}
    }
}