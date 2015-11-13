namespace Net.Sf.Dbdeploy
{
    using System;
    using System.IO;

    using NAnt.Core;
    using NAnt.Core.Attributes;

    using Net.Sf.Dbdeploy.Configuration;
    using Net.Sf.Dbdeploy.Database;
    using Net.Sf.Dbdeploy.Exceptions;

    [TaskName("dbdeploy")]
    public class NAntTask : Task
    {
        private readonly DbDeployConfig config;

        public NAntTask()
        {
            this.config = new DbDeployConfig();
        }
        
        [TaskAttribute("dbType", Required = true)]
        public string DbType
        {
            set { this.config.Dbms = value; }
        }
        
        [TaskAttribute("dbConnection", Required = true)]
        public string DbConnection
        {
            set { this.config.ConnectionString = value; }
        }
        
        [TaskAttribute("dir", Required = true)]
        public DirectoryInfo Dir
        {
            get { return this.config.ScriptDirectory; }
            set { this.config.ScriptDirectory = value; }
        }
        
        [TaskAttribute("outputFile")]
        public FileInfo OutputFile
        {
            get { return this.config.OutputFile; }
            set { this.config.OutputFile = value; }
        }
        
        [TaskAttribute("encoding")]
        public string OutputEncoding
        {
            get { return this.config.Encoding.EncodingName; }
            set { this.config.Encoding = new OutputFileEncoding(value).AsEncoding(); }
        }
        
        [TaskAttribute("undoOutputFile")]
        public FileInfo UndoOutputFile
        {
            get { return this.config.UndoOutputFile; }
            set { this.config.UndoOutputFile = value; }
        }
        
        [TaskAttribute("templateDir")]
        public DirectoryInfo TemplateDir
        {
            get { return this.config.TemplateDirectory; }
            set { this.config.TemplateDirectory = value; }
        }
        
        [TaskAttribute("lastChangeToApply")]
        public string LastChangeToApply
        {
            get { return this.config.LastChangeToApply != null ? this.config.LastChangeToApply.UniqueKey : string.Empty; }
            set { this.config.LastChangeToApply = string.IsNullOrWhiteSpace(value) ? null : new UniqueChange(value); }
        }
        
        [TaskAttribute("changeLogTable")]
        public string ChangeLogTable
        {
            get { return this.config.ChangeLogTableName; }
            set { this.config.ChangeLogTableName = value; }
        }

        /// <summary>
        /// Gets or sets if the change log table should be automatically created if it does not exist.
        /// </summary>
        /// <value>
        /// The auto create change table.
        /// </value>
        [TaskAttribute("autoCreateChangeLogTable")]
        public bool AutoCreateChangeLogTable
        {
            get { return this.config.AutoCreateChangeLogTable; }
            set { this.config.AutoCreateChangeLogTable = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to retry and previously failed scripts.
        /// </summary>
        /// <value>
        ///   <c>true</c> if force update; otherwise, <c>false</c>.
        /// </value>
        [TaskAttribute("forceUpdate")]
        public bool ForceUpdate
        {
            get { return this.config.ForceUpdate; }
            set { this.config.ForceUpdate = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use SQLCMD mode.
        /// </summary>
        /// <value>
        ///   <c>true</c> if use SQL CMD; otherwise, <c>false</c>.
        /// </value>
        [TaskAttribute("useSqlCmd")]
        public bool UseSqlCmd
        {
            get { return this.config.UseSqlCmd; }
            set { this.config.UseSqlCmd = value; }
        }
        
        [TaskAttribute("delimiter")]
        public string Delimiter
        {
            get { return this.config.Delimiter; }
            set { this.config.Delimiter = value; }
        }
        
        [TaskAttribute("delimiterType")]
        public string DelimiterType
        {
            get { return this.config.DelimiterType.GetType().Name; }
            set { this.config.DelimiterType = DelimiterTypeFactory.Create(value); }
        }

        protected override void ExecuteTask()
        {
            try
            {
                var deployer = new DbDeployer();
                deployer.Execute(this.config, Console.Out);
            }
            catch (UsageException ex)
            {
                Console.Error.WriteLine(ex.Message);

                this.PrintUsage();
            }
            catch (DbDeployException ex)
            {
                Console.Error.WriteLine(ex.Message);

                throw new BuildException(ex.Message);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Failed to apply changes: " + ex);
                Console.Error.WriteLine("Stack Trace:");
                Console.Error.Write(ex.StackTrace);

                throw new BuildException(ex.Message);
            }
        }

        public void PrintUsage()
        {
            string message = "\n\nDbdeploy Ant Task Usage"
                + "\n======================="
                + "\n\n\t<dbdeploy"
                + "\n\t\tdbType=\"[DATABASE TYPE - mssql/mysql/ora/postgres]\" *"
                + "\n\t\tdbConnection=\"[DATABASE CONNECTION STRING]\" *"
                + "\n\t\ttemplatedir=\"[DIRECTORY FOR DBMS TEMPLATE SCRIPTS, IF NOT USING BUILT-IN]\""
                + "\n\t\tdir=\"[YOUR SCRIPT FOLDER]\" *"
                + "\n\t\tencoding=\"[CHARSET OF IN- AND OUTPUT SQL SCRIPTS - default UTF-8]\""
                + "\n\t\toutputfile=\"[OUTPUT SCRIPT PATH + NAME]\""
                + "\n\t\tlastChangeToApply=\"[NUMBER OF THE LAST SCRIPT TO APPLY]\""
                + "\n\t\tundoOutputfile=\"[UNDO SCRIPT PATH + NAME]\""
                + "\n\t\tchangeLogTableName=\"[CHANGE LOG TABLE NAME]\""
                + "\n\t\tdelimiter=\"[STATEMENT DELIMITER - default ;]\""
                + "\n\t\tdelimitertype=\"[STATEMENT DELIMITER TYPE - row or normal, default normal]\""
                + "\n\t/>"
                + "\n\n* - Indicates mandatory parameter";

            Console.Out.WriteLine(message);
        }
    }
}