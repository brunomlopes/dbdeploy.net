using System;
using System.IO;
using System.Text;
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
        private int currentDbVersion = Int32.MinValue;
    	private string changeLogTable = DatabaseSchemaVersionManager.DEFAULT_TABLE_NAME;
    	private bool useTransaction = false;
        private string outputFileEncoding = string.Empty;

        [TaskAttribute("dbType", Required = true)]
        public string DbType
        {
            set { dbType = value; }
        }

        [TaskAttribute("dbConnection")]
        public string DbConnection
        {
            set { dbConnection = value; }
        }

        [TaskAttribute("dir", Required=true)]
        public DirectoryInfo Dir
        {
            set { dir = value; }
        }

        [TaskAttribute("outputFile", Required = true)]
        public FileInfo Outputfile
        {
            set { outputfile = value; }
        }

        [TaskAttribute("outputFileEncoding")]
        public string OutputFileEncoding
        {
            set { outputFileEncoding = value;}
        }
       
        [TaskAttribute("undoOutputFile")]
        public FileInfo UndoOutputfile
        {
            set { undoOutputfile = value; }
        }

        [TaskAttribute("lastChangeToApply")]
        public int LastChangeToApply
        {
            set { lastChangeToApply = value; }
        }

        [TaskAttribute("deltaSet")]
        public string DeltaSet
        {
            set { deltaSet = value; }
        }

        [TaskAttribute("currentDbVersion")]
        public int CurrentDbVersion
        {
            set { currentDbVersion = value; }
        }

		[TaskAttribute("changeLogTable")]
		public string ChangeLogTable
    	{
			set { changeLogTable = value; }
    	}

		[TaskAttribute("useTransaction")]
		public bool UseTransaction
		{
			set { useTransaction = value; }
		}

        private int? GetCurrentDbVersion()
        {
            if (currentDbVersion < 0)
				return null;

            return currentDbVersion;
        }
        
        protected override void InitializeTask(XmlNode taskNode)
        {
            Validator validator = new Validator();
            validator.SetUsage("nant");
            validator.Validate(dbConnection, dbType, dir.FullName, outputfile.Name, GetCurrentDbVersion());
//            TODO: Add validation for the OutputFileEncoding
        }

        protected override void ExecuteTask()
        {
            try
            {
                Encoding encoding = new OutputFileEncoding(outputFileEncoding).AsEncoding();
                using (TextWriter outputPrintStream = new StreamWriter(outputfile.FullName, true, encoding))
                {
                    TextWriter undoOutputPrintStream = null;
                    if (undoOutputfile != null)
                        undoOutputPrintStream = new StreamWriter(undoOutputfile.FullName, true, encoding);

                    DbmsFactory factory = new DbmsFactory(dbType, dbConnection);
                    IDbmsSyntax dbmsSyntax = factory.CreateDbmsSyntax();
                    DatabaseSchemaVersionManager databaseSchemaVersion = new DatabaseSchemaVersionManager(factory, deltaSet, GetCurrentDbVersion(), changeLogTable);

                    ToPrintStreamDeployer toPrintSteamDeployer = new ToPrintStreamDeployer(databaseSchemaVersion, dir, outputPrintStream, dbmsSyntax, useTransaction, undoOutputPrintStream);
                    toPrintSteamDeployer.DoDeploy(lastChangeToApply);

                    if (undoOutputPrintStream != null)
                        undoOutputPrintStream.Close();
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