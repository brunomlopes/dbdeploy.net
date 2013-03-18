using System.IO;
using System.Management.Automation;

namespace Dbdeploy.Powershell.Commands
{
    public class DbUpdateBase : PSCmdlet
    {
        private const string DatabaseTypeDefault = "mssql";
        private const string TableNameDefault = "ChangeLog";

        private XmlConfiguration config;

        protected string deltasDirectory;
        private string tableName = TableNameDefault;

        private string databaseType = DatabaseTypeDefault;

        [Parameter(Mandatory = false)]
        public string ConfigurationFile { get; set; }

        [Parameter(Mandatory = true, Position = 0)]
        public string DeltasDirectory { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Defaults to mssql")]
        public string DatabaseType
        {
            get { return this.databaseType; }
            set { this.databaseType = value; }
        }

        [Parameter(Mandatory = false)]
        public string ConnectionString { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Changelog table name. Defaults to ChangeLog")]
        public string TableName
        {
            get { return this.tableName; }
            set { this.tableName = value; }
        }

        [Parameter(Mandatory = false, HelpMessage = "Sets if the Changelog table should be automatically created. Defaults to true")]
        public bool AutoCreateChangeLogTable { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Sets if SQLCMD mode should be used. Defaults to false")]
        public bool UseSqlCmd { get; set; }

        protected override void ProcessRecord()
        {
            var configurationFile = this.ToAbsolutePath(ConfigurationFile);
            this.deltasDirectory = this.ToAbsolutePath(DeltasDirectory);

            if (!string.IsNullOrEmpty(configurationFile) && File.Exists(configurationFile))
            {
                this.config = new XmlConfiguration(configurationFile);

                if (string.IsNullOrEmpty(this.DatabaseType) || this.DatabaseType == DatabaseTypeDefault)
                    this.DatabaseType = this.config.DbType;

                if (string.IsNullOrEmpty(this.ConnectionString))
                    this.ConnectionString = this.config.DbConnectionString;

                if (string.IsNullOrEmpty(this.TableName) || this.TableName == TableNameDefault)
                    this.TableName = this.config.TableName;
            }

            if (string.IsNullOrEmpty(this.ConnectionString))
            {
                throw new InvalidDataException(
                    "Missing connection string. It must either be in the config file or passed as a parameter");
            }
        }

        protected string ToAbsolutePath(string deltasDirectory)
        {
            if (string.IsNullOrEmpty(deltasDirectory))
                return null;

            if (!Path.IsPathRooted(deltasDirectory))
            {
                deltasDirectory = Path.Combine(this.SessionState.Path.CurrentFileSystemLocation.Path, deltasDirectory);
            }

            return deltasDirectory;
        }
    }
}