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
        protected XmlConfiguration _config;
        protected string _configurationFile;

        protected override void ProcessRecord()
        {
            _configurationFile = ToAbsolutePath(ConfigurationFile);
            _deltasDirectory = ToAbsolutePath(DeltasDirectory);


            _config = new XmlConfiguration(_configurationFile);
            _dbmsFactory = new DbmsFactory(_config.DbType, _config.DbConnectionString, ForDirectExecution);
            _databaseSchemaVersion = new DatabaseSchemaVersionManager(_dbmsFactory,
                                                                     _config.DbDeltaSet,
                                                                     _config.CurrentDbVersion,
                                                                     _config.TableName);

        }

        protected virtual bool ForDirectExecution { get { return false; } }

        protected string ToAbsolutePath(string deltasDirectory)
        {
            if (!Path.IsPathRooted(deltasDirectory))
            {
                deltasDirectory = Path.Combine(SessionState.Path.CurrentFileSystemLocation.Path, deltasDirectory);
            }
            return deltasDirectory;
        }

        [Parameter(Mandatory = true, Position = 0)]
        public string ConfigurationFile { get; set; }

        [Parameter(Mandatory = true, Position = 1)]
        public string DeltasDirectory { get; set; }
    }
}