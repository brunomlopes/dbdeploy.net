using System;
using System.IO;
using System.Xml;
using NAnt.Core;
using NAnt.Core.Attributes;
using Net.Sf.Dbdeploy.Database;
using Net.Sf.Dbdeploy.Exceptions;

namespace Net.Sf.Dbdeploy
{
    [TaskName("dbdeploy")]
    public class NAntTask : Task
    {
        private string dbType;
        private string dbConnection;
        private DirectoryInfo dir;
        private FileInfo outputfile;
        private FileInfo undoOutputfile;

        private int lastChangeToApply = Int32.MaxValue;
        private String deltaSet = "Main";

        [TaskAttribute("dbType", Required = true)]
        public string DbType
        {
            set { dbType = value; }
        }

        [TaskAttribute("dbConnection", Required = true)]
        public string DbConnection
        {
            set { dbConnection = value; }
        }

        [TaskAttribute("dir", Required=true)]
        public DirectoryInfo Dir
        {
            get { return dir; }
            set { dir = value; }
        }

        [TaskAttribute("outputFile", Required = true)]
        public FileInfo Outputfile
        {
            get { return outputfile; }
            set { outputfile = value; }
        }
       
        [TaskAttribute("undoOutputFile")]
        public FileInfo UndoOutputfile
        {
            get { return undoOutputfile; }
            set { undoOutputfile = value; }
        }

        [TaskAttribute("lastChangeToApply")]
        public int LastChangeToApply
        {
            get { return lastChangeToApply; }
            set { lastChangeToApply = value; }
        }

        [TaskAttribute("deltaSet")]
        public string DeltaSet
        {
            get { return deltaSet; }
            set { deltaSet = value; }
        }

        protected override void InitializeTask(XmlNode taskNode)
        {
            Validator validator = new Validator();
            validator.SetUsage("nant");
            validator.Validate(dbConnection, dbType, dir.FullName, outputfile.Name);
        }

        protected override void ExecuteTask()
        {
            try
            {
                using (TextWriter outputPrintStream = new StreamWriter(outputfile.FullName))
                {
                    TextWriter undoOutputPrintStream = null;
                    if (undoOutputfile != null)
                    {
                        undoOutputPrintStream = new StreamWriter(undoOutputfile.FullName);
                    }
                    DbmsFactory factory = new DbmsFactory(dbType, dbConnection);
                    DbmsSyntax dbmsSyntax = factory.CreateDbmsSyntax();
                    DatabaseSchemaVersionManager databaseSchemaVersion =new DatabaseSchemaVersionManager(factory, deltaSet);

                    ToPrintStreamDeployer toPrintSteamDeployer 
                        = new ToPrintStreamDeployer(databaseSchemaVersion, dir, outputPrintStream, dbmsSyntax,undoOutputPrintStream);
                    toPrintSteamDeployer.doDeploy(lastChangeToApply);

                    if (undoOutputPrintStream != null)
                    {
                        undoOutputPrintStream.Close();
                    }
                }
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
    }
}