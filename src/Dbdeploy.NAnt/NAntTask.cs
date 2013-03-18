using System;
using System.IO;
using NAnt.Core;
using NAnt.Core.Attributes;
using Net.Sf.Dbdeploy.Database;
using Net.Sf.Dbdeploy.Exceptions;

namespace Net.Sf.Dbdeploy
{
    [TaskName("dbdeploy")]
    public class NAntTask : Task
    {
        private readonly DbDeployer dbDeploy;

        public NAntTask()
        {
            this.dbDeploy = new DbDeployer();

            this.dbDeploy.InfoWriter = Console.Out;
        }
        
        [TaskAttribute("dbType", Required = true)]
        public string DbType
        {
            set { this.dbDeploy.Dbms = value; }
        }
        
        [TaskAttribute("dbConnection", Required = true)]
        public string DbConnection
        {
            set { this.dbDeploy.ConnectionString = value; }
        }
        
        [TaskAttribute("dir", Required = true)]
        public DirectoryInfo Dir
        {
            get { return this.dbDeploy.ScriptDirectory; }
            set { this.dbDeploy.ScriptDirectory = value; }
        }
        
        [TaskAttribute("outputFile")]
        public FileInfo OutputFile
        {
            get { return this.dbDeploy.OutputFile; }
            set { this.dbDeploy.OutputFile = value; }
        }
        
        [TaskAttribute("encoding")]
        public string OutputEncoding
        {
            get { return this.dbDeploy.Encoding.EncodingName; }
            set { this.dbDeploy.Encoding = new OutputFileEncoding(value).AsEncoding(); }
        }
        
        [TaskAttribute("undoOutputFile")]
        public FileInfo UndoOutputFile
        {
            get { return this.dbDeploy.UndoOutputFile; }
            set { this.dbDeploy.UndoOutputFile = value; }
        }
        
        [TaskAttribute("templateDir")]
        public DirectoryInfo TemplateDir
        {
            get { return this.dbDeploy.TemplateDir; }
            set { this.dbDeploy.TemplateDir = value; }
        }
        
        [TaskAttribute("lastChangeToApply")]
        public int? LastChangeToApply
        {
            get { return this.dbDeploy.LastChangeToApply; }
            set { this.dbDeploy.LastChangeToApply = value; }
        }
        
        [TaskAttribute("changeLogTable")]
        public string ChangeLogTable
        {
            get { return this.dbDeploy.ChangeLogTableName; }
            set { this.dbDeploy.ChangeLogTableName = value; }
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
            get { return this.dbDeploy.AutoCreateChangeLogTable; }
            set { this.dbDeploy.AutoCreateChangeLogTable = value; }
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
            get { return this.dbDeploy.UseSqlCmd; }
            set { this.dbDeploy.UseSqlCmd = value; }
        }
        
        [TaskAttribute("delimiter")]
        public string Delimiter
        {
            get { return this.dbDeploy.Delimiter; }
            set { this.dbDeploy.Delimiter = value; }
        }
        
        [TaskAttribute("delimiterType")]
        public string DelimiterType
        {
            get { return this.dbDeploy.DelimiterType.GetType().Name; }
            set { this.dbDeploy.DelimiterType = DelimiterTypeFactory.Create(value); }
        }

        protected override void ExecuteTask()
        {
            try
            {
                this.dbDeploy.Go();
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
                + "\n\t\tdbType=\"[DATABASE TYPE - mssql/mysql/ora]\" *"
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