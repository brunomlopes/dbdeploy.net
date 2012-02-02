using System;
using System.IO;
using Microsoft.Build.Framework;
using Net.Sf.Dbdeploy;
using Net.Sf.Dbdeploy.Database;
using Net.Sf.Dbdeploy.Exceptions;

namespace MSBuild.Dbdeploy.Task
{
    public class Dbdeploy : ITask
    {
        private readonly DbDeployer dbDeploy;
                        
        public Dbdeploy()
        {
            this.dbDeploy = new DbDeployer();

            this.dbDeploy.InfoWriter = Console.Error;
        }

        [Required]
        public string Dbms
        {
            set { this.dbDeploy.Dbms = value; }
        }

        [Required]
        public string DbConnection
        {
            set { this.dbDeploy.ConnectionString = value; }
        }

        [Required]
        public string Dir
        {
            get { return this.dbDeploy.ScriptDirectory.FullName; }
            set { this.dbDeploy.ScriptDirectory = new DirectoryInfo(value); }
        }

        [Required]
        public string OutputFile
        {
            get { return this.dbDeploy.OutputFile.FullName; }
            set { this.dbDeploy.OutputFile = new FileInfo(value); }
        }

        public string UndoOutputFile
        {
            get { return this.dbDeploy.UndoOutputFile.FullName; }
            set { this.dbDeploy.UndoOutputFile = new FileInfo(value); }
        }

        public string TemplateDir
        {
            get { return this.dbDeploy.TemplateDir.FullName; }
            set { this.dbDeploy.TemplateDir = new DirectoryInfo(value); }
        }

        public int? LastChangeToApply
        {
            get { return this.dbDeploy.LastChangeToApply; }
            set { this.dbDeploy.LastChangeToApply = value; }
        }

        public string TableName
        {
            get { return this.dbDeploy.ChangeLogTableName; }
            set { this.dbDeploy.ChangeLogTableName = value; }
        }

        public string Delimiter
        {
            get { return this.dbDeploy.Delimiter; }
            set { this.dbDeploy.Delimiter = value; }
        }

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

        public IBuildEngine BuildEngine { get; set; }

        public ITaskHost HostObject { get; set; }

        public bool Execute()
        {
            try
            {
                this.dbDeploy.Go();

                return true;
            }
            catch (UsageException ex)
            {
                Console.Error.WriteLine(ex.Message);

                this.PrintUsage();
            }
            catch (DbDeployException ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Failed to apply changes: " + ex.Message);
                Console.Error.WriteLine("Stack Trace:");
                Console.Error.Write(ex.StackTrace);
            }

            return false;
        }

        public void PrintUsage()
        {
            string message = "\n\nDbdeploy MSBuild Task Usage"
                + "\n======================="
                + "\n\n\t<Dbdeploy"
                + "\n\t\tDbConnection=\"[DATABASE CONECTIONSTRING]\" *"
                + "\n\t\tdbms=\"[YOUR DBMS]\" *"
                + "\n\t\ttemplatedir=\"[DIRECTORY FOR DBMS TEMPLATE SCRIPTS, IF NOT USING BUILT-IN]\""
                + "\n\t\tdir=\"[YOUR SCRIPT FOLDER]\" *"
                + "\n\t\toutputfile=\"[OUTPUT SCRIPT PATH + NAME]\""
                + "\n\t\tlastChangeToApply=\"[NUMBER OF THE LAST SCRIPT TO APPLY]\""
                + "\n\t\tundoOutputfile=\"[UNDO SCRIPT PATH + NAME]\""
                + "\n\t\tchangeLogTableName=\"[CHANGE LOG TABLE NAME - default changelog]\""
                + "\n\t\tdelimiter=\"[STATEMENT DELIMITER - default ;]\""
                + "\n\t\tdelimitertype=\"[STATEMENT DELIMITER TYPE - row or normal, default normal]\""
                + "\n\t/>"
                + "\n\n* - Indicates mandatory parameter";

            Console.Out.WriteLine(message);
        }
    }
}