using System.Linq;

namespace MSBuild.Dbdeploy.Task
{
    using System;
    using System.IO;

    using Microsoft.Build.Framework;

    using Net.Sf.Dbdeploy;
    using Net.Sf.Dbdeploy.Configuration;
    using Net.Sf.Dbdeploy.Database;
    using Net.Sf.Dbdeploy.Exceptions;

    public class Dbdeploy : ITask
    {
        private readonly DbDeployConfig config;
                        
        public Dbdeploy()
        {
            this.config = new DbDeployConfig();
        }

        [Required]
        public string DbType
        {
            set { this.config.Dbms = value; }
        }

        [Required]
        public string DbConnection
        {
            set { this.config.ConnectionString = value; }
        }

        [Required]
        public string Dir
        {
            get { return string.Join(";", this.config.ScriptDirectory.Select(x => x.FullName).ToArray()); }
            set { this.config.ScriptDirectory = Parser.ParseDirectory(value); }
        }

        public string OutputFile
        {
            get { return this.config.OutputFile.FullName; }
            set { this.config.OutputFile = new FileInfo(value); }
        }

        public string Encoding
        {
            get { return this.config.Encoding.EncodingName; }
            set { this.config.Encoding = new OutputFileEncoding(value).AsEncoding(); }
        }

        public string UndoOutputFile
        {
            get { return this.config.UndoOutputFile.FullName; }
            set { this.config.UndoOutputFile = new FileInfo(value); }
        }

        public string TemplateDir
        {
            get { return this.config.TemplateDirectory.FullName; }
            set { this.config.TemplateDirectory = new DirectoryInfo(value); }
        }

        public string LastChangeToApply
        {
            get { return this.config.LastChangeToApply != null ? this.config.LastChangeToApply.ToString() : string.Empty; }
            set { this.config.LastChangeToApply = string.IsNullOrWhiteSpace(value) ? null : new UniqueChange(value); }
        }

        public string TableName
        {
            get { return this.config.ChangeLogTableName; }
            set { this.config.ChangeLogTableName = value; }
        }

        public bool AutoCreateChangeLogTable
        {
            get { return this.config.AutoCreateChangeLogTable; }
            set { this.config.AutoCreateChangeLogTable = value; }
        }

        public bool UseSqlCmd
        {
            get { return this.config.UseSqlCmd; }
            set { this.config.UseSqlCmd = value; }
        }

        public string Delimiter
        {
            get { return this.config.Delimiter; }
            set { this.config.Delimiter = value; }
        }

        public string DelimiterType
        {
            get { return this.config.DelimiterType.GetType().Name; }
            set { this.config.DelimiterType = DelimiterTypeFactory.Create(value); }
        }

        public IBuildEngine BuildEngine { get; set; }

        public ITaskHost HostObject { get; set; }

        public bool Execute()
        {
            try
            {
                var deployer = new DbDeployer();
                deployer.Execute(this.config, Console.Out);

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