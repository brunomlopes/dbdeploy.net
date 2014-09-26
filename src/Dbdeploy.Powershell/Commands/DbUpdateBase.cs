using System.IO;
using System.Management.Automation;

namespace Dbdeploy.Powershell.Commands
{
    using System.Linq;

    using Net.Sf.Dbdeploy.Configuration;

    public class DbUpdateBase : PSCmdlet
    {
        private DbDeployConfig config;

        protected string deltasDirectory;
        private string tableName = DbDeployDefaults.ChangeLogTableName;

        private string databaseType = DbDeployDefaults.Dbms;

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

        [Parameter(Mandatory = false, HelpMessage = "Sets if previously failed scripts should be retried. Defaults to false")]
        public bool ForceUpdate { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Sets if SQLCMD mode should be used. Defaults to false")]
        public bool UseSqlCmd { get; set; }

        protected override void ProcessRecord()
        {
            var configurationFile = this.ToAbsolutePath(ConfigurationFile);
            this.deltasDirectory = this.ToAbsolutePath(DeltasDirectory);

            if (!string.IsNullOrEmpty(configurationFile) && File.Exists(configurationFile))
            {
                var configurationManager = new DbDeployConfigurationManager();
                this.config = configurationManager.ReadConfiguration(configurationFile).Deployments.FirstOrDefault();

                if (string.IsNullOrEmpty(this.DatabaseType) || this.DatabaseType == DbDeployDefaults.Dbms)
                    this.DatabaseType = this.config.Dbms;

                if (string.IsNullOrEmpty(this.ConnectionString))
                    this.ConnectionString = this.config.ConnectionString;

                if (string.IsNullOrEmpty(this.TableName) || this.TableName == DbDeployDefaults.ChangeLogTableName)
                    this.TableName = this.config.ChangeLogTableName;
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

        protected DbDeployConfig ConfigFromParameters()
        {
            return new DbDeployConfig
            {
                Dbms = this.DatabaseType,
                ConnectionString = this.ConnectionString,
                ChangeLogTableName = this.TableName,
                ScriptDirectory = new DirectoryInfo(this.deltasDirectory),
                AutoCreateChangeLogTable = this.AutoCreateChangeLogTable,
                ForceUpdate = this.ForceUpdate,
                UseSqlCmd = this.UseSqlCmd
            };
        }
    }
}