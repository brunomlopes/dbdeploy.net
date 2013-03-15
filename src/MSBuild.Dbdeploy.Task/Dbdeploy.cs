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

            this.dbDeploy.InfoWriter = Console.Out;
        }

        [Required]
        public string DbType
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

        public string OutputFile
        {
            get { return this.dbDeploy.OutputFile.FullName; }
            set { this.dbDeploy.OutputFile = new FileInfo(value); }
        }

        public string Encoding
        {
            get { return this.dbDeploy.Encoding.EncodingName; }
            set { this.dbDeploy.Encoding = new OutputFileEncoding(value).AsEncoding(); }
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

        public int LastChangeToApply
        {
            get { return this.dbDeploy.LastChangeToApply ?? -1; }
            set { this.dbDeploy.LastChangeToApply = value < 0 ? default(int?) : value; }
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
            get { return this.dbDeploy.DelimiterType.GetType().Name; }
            set { this.dbDeploy.DelimiterType = DelimiterTypeFactory.Create(value); }
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
            string message = Environment.NewLine 
                + Environment.NewLine + "Dbdeploy MSBuild Task Usage"
                + Environment.NewLine + "======================="
                + Environment.NewLine 
                + Environment.NewLine + "\t<Dbdeploy"
                + Environment.NewLine + "\t\tDbConnection=\"[DATABASE CONECTIONSTRING]\" *"
                + Environment.NewLine + "\t\tdbms=\"[YOUR DBMS]\" *"
                + Environment.NewLine + "\t\ttemplatedir=\"[DIRECTORY FOR DBMS TEMPLATE SCRIPTS, IF NOT USING BUILT-IN]\""
                + Environment.NewLine + "\t\tdir=\"[YOUR SCRIPT FOLDER]\" *"
                + Environment.NewLine + "\t\toutputfile=\"[OUTPUT SCRIPT PATH + NAME]\""
                + Environment.NewLine + "\t\tlastChangeToApply=\"[NUMBER OF THE LAST SCRIPT TO APPLY]\""
                + Environment.NewLine + "\t\tundoOutputfile=\"[UNDO SCRIPT PATH + NAME]\""
                + Environment.NewLine + "\t\tchangeLogTableName=\"[CHANGE LOG TABLE NAME - default ChangeLog]\""
                + Environment.NewLine + "\t\tdelimiter=\"[STATEMENT DELIMITER - default ;]\""
                + Environment.NewLine + "\t\tdelimitertype=\"[STATEMENT DELIMITER TYPE - row or normal, default normal]\""
                + Environment.NewLine + "\t/>"
                + Environment.NewLine 
                + Environment.NewLine + "* - Indicates mandatory parameter";

            Console.Out.WriteLine(message);
        }
    }
}