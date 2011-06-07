using System;
using System.IO;
using System.Management.Automation;
using Net.Sf.Dbdeploy.Configuration;
using Net.Sf.Dbdeploy.Database;

namespace Dbdeploy.Powershell.Commands
{
    public class DbUpdateBase : PSCmdlet
    {
        protected DatabaseSchemaVersionManager _databaseSchemaVersion;
        protected string _deltasDirectory;
        protected DbmsFactory _dbmsFactory;
        private XmlConfiguration _config;

        protected override void ProcessRecord()
        {
            var configurationFile = ToAbsolutePath(ConfigurationFile);
            _deltasDirectory = ToAbsolutePath(DeltasDirectory);

            if (!string.IsNullOrEmpty(configurationFile) && File.Exists(configurationFile))
            {
                _config = new XmlConfiguration(configurationFile);
                if(string.IsNullOrEmpty(DatabaseType))
                    DatabaseType = _config.DbType;
                if(string.IsNullOrEmpty(ConnectionString))
                    ConnectionString = _config.DbConnectionString;
                if(string.IsNullOrEmpty(DeltaSet))
                    DeltaSet = _config.DbDeltaSet;
                if(string.IsNullOrEmpty(TableName))
                    TableName = _config.TableName;
                if(!CurrentDbVersion.HasValue)
                    CurrentDbVersion = _config.CurrentDbVersion;
            }

            _dbmsFactory = new DbmsFactory(DatabaseType, ConnectionString, ForDirectExecution);
            _databaseSchemaVersion = new DatabaseSchemaVersionManager(_dbmsFactory,
                                                                     DeltaSet,
                                                                     CurrentDbVersion,
                                                                     TableName);
        }

        protected virtual bool ForDirectExecution { get { return false; } }

        protected string ToAbsolutePath(string deltasDirectory)
        {
            if (string.IsNullOrEmpty(deltasDirectory))
                return null;
            if (!Path.IsPathRooted(deltasDirectory))
            {
                deltasDirectory = Path.Combine(SessionState.Path.CurrentFileSystemLocation.Path, deltasDirectory);
            }
            return deltasDirectory;
        }


        [Parameter(Mandatory = false, Position = 0)]
        public string ConfigurationFile { get; set; }

        [Parameter(Mandatory = true, Position = 1)]
        public string DeltasDirectory { get; set; }

        [Parameter(Mandatory = false)]
        public string DatabaseType { get; set; }
        
        [Parameter(Mandatory = false)]
        public string ConnectionString { get; set; }

        private bool _useTransaction = false;

        [Parameter(Mandatory = false, HelpMessage = "Defaults to false")]
        public bool UseTransaction
        {
            get { return _useTransaction; }
            set { _useTransaction = value; }
        }
        
        private string _deltaSet = "Main";

        [Parameter(Mandatory = false, HelpMessage = "Defaults to 'Main'")]
        public string DeltaSet
        {
            get { return _deltaSet; }
            set { _deltaSet = value; }
        }

        [Parameter(Mandatory = false, HelpMessage = "If not set, fetches current version from database")]
        public int? CurrentDbVersion { get; set; }

        private string _tableName = "changelog";

        [Parameter(Mandatory = false, HelpMessage = "Changelog table name. Defaults to changelog")]
        public string TableName
        {
            get { return _tableName; }
            set { _tableName = value; }
        }
    }
}