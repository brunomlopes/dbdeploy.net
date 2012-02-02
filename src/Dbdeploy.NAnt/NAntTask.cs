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
        public string Dir
        {
            get { return this.dbDeploy.ScriptDirectory.FullName; }
            set { this.dbDeploy.ScriptDirectory = new DirectoryInfo(value); }
        }
        
        [TaskAttribute("outputFile")]
        public string OutputFile
        {
            get { return this.dbDeploy.OutputFile.FullName; }
            set { this.dbDeploy.OutputFile = new FileInfo(value); }
        }
        
        [TaskAttribute("outputFileEncoding")]
        public string OutputEncoding
        {
            get { return this.dbDeploy.OutputEncoding.EncodingName; }
            set { this.dbDeploy.OutputEncoding = new OutputFileEncoding(value).AsEncoding(); }
        }
        
        [TaskAttribute("undoOutputFile")]
        public string UndoOutputFile
        {
            get { return this.dbDeploy.UndoOutputFile.FullName; }
            set { this.dbDeploy.UndoOutputFile = new FileInfo(value); }
        }
        
        [TaskAttribute("templateDir")]
        public string TemplateDir
        {
            get { return this.dbDeploy.TemplateDir.FullName; }
            set { this.dbDeploy.TemplateDir = new DirectoryInfo(value); }
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
        
        [TaskAttribute("delimiter")]
        public string Delimiter
        {
            get { return this.dbDeploy.Delimiter; }
            set { this.dbDeploy.Delimiter = value; }
        }
        
        [TaskAttribute("delimiterType")]
        public string DelimiterType
        {
            get
            {
                return this.dbDeploy.DelimiterType.GetType().Name;
            }

            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                switch (value.ToUpperInvariant())
                {
                    case "ROW":
                        this.dbDeploy.DelimiterType = new RowDelimiter();
                        break;

                    case "NORMAL":
                    default:
                        this.dbDeploy.DelimiterType = new NormalDelimiter();
                        break;
                }
            }
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
                + "\n\t\toutputfileencoding=\"[CHARSET OF OUTPUT SQL SCRIPTS - default UTF-8]\""
                + "\n\t\toutputfile=\"[OUTPUT SCRIPT PATH + NAME]\""
                + "\n\t\tlastChangeToApply=\"[NUMBER OF THE LAST SCRIPT TO APPLY]\""
                + "\n\t\tundoOutputfile=\"[UNDO SCRIPT PATH + NAME]\""
                + "\n\t\tchangeLogTableName=\"[CHANGE LOG TABLE NAME]\""
                + "\n\t\tdelimiter=\"[STATEMENT DELIMITER - default ;]\""
                + "\n\t\tdelimitertype=\"[STATEMENT DELIMITER TYPE - row or normal, default normal]\""
                + "\n\t/>"
                + "\n\n* - Indicates mandatory parameter"; ;

            Console.Out.WriteLine(message);
        }
    }
}