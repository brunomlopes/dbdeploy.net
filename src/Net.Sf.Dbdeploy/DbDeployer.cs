using System;
using System.IO;
using System.Reflection;
using Net.Sf.Dbdeploy.Appliers;
using Net.Sf.Dbdeploy.Database;
using Net.Sf.Dbdeploy.Exceptions;
using Net.Sf.Dbdeploy.Scripts;
using System.Text;

namespace Net.Sf.Dbdeploy
{
    public class DbDeployer
    {
        public DbDeployer()
        {
            this.Encoding = Encoding.UTF8;
            this.LineEnding = Database.LineEnding.Platform;
            this.ChangeLogTableName = "changelog";
            this.Delimiter = ";";
            this.DelimiterType = new NormalDelimiter();
        }
        
        public string Dbms { get; set; }

        public string ConnectionString { get; set; }

        public DirectoryInfo ScriptDirectory { get; set; }

        public FileInfo OutputFile { get; set; }

        public FileInfo UndoOutputFile { get; set; }

        public Encoding Encoding { get; set; }

        public string LineEnding { get; set; }

        public int? LastChangeToApply { get; set; }

        public string ChangeLogTableName { get; set; }

        public string Delimiter { get; set; }

        public IDelimiterType DelimiterType  { get; set; }

        public DirectoryInfo TemplateDir { get; set; }

        public TextWriter InfoWriter { get; set; }

        public string GenerateWelcomeString()
        {
            Version version = Assembly.GetAssembly(this.GetType()).GetName().Version;
            
            return "dbdeploy.net " + version;
        }

        public void Go()
        {
            this.Validate();

            this.InfoWriter.WriteLine(this.GenerateWelcomeString());
            
            var factory = new DbmsFactory(this.Dbms, this.ConnectionString);
            
            var dbmsSyntax = factory.CreateDbmsSyntax();

            QueryExecuter queryExecuter = new QueryExecuter(factory);

            var databaseSchemaVersionManager =
                    new DatabaseSchemaVersionManager(queryExecuter, dbmsSyntax, this.ChangeLogTableName);

            var scanner = new DirectoryScanner(this.InfoWriter, this.Encoding);

            var changeScriptRepository =
                    new ChangeScriptRepository(scanner.GetChangeScriptsForDirectory(this.ScriptDirectory));

            IChangeScriptApplier doScriptApplier;
            TextWriter doWriter = null;

            if (this.OutputFile != null) 
            {
                doWriter = new StreamWriter(this.OutputFile.OpenWrite(), this.Encoding);

                doScriptApplier = new TemplateBasedApplier(
                    doWriter, 
                    this.Dbms, 
                    this.ChangeLogTableName, 
                    this.Delimiter, 
                    this.DelimiterType, 
                    this.TemplateDir);
            } 
            else 
            {
                QueryStatementSplitter splitter = new QueryStatementSplitter
                {
                    Delimiter = this.Delimiter,
                    DelimiterType = this.DelimiterType,
                    LineEnding = this.LineEnding,
                };

                doScriptApplier = new DirectToDbApplier(
                    queryExecuter, 
                    databaseSchemaVersionManager, 
                    splitter, 
                    this.InfoWriter);
            }

            IChangeScriptApplier undoScriptApplier = null;
            TextWriter undoWriter = null;

            if (this.UndoOutputFile != null) 
            {
                undoWriter = new StreamWriter(this.UndoOutputFile.OpenWrite(), this.Encoding);

                undoScriptApplier = new UndoTemplateBasedApplier(
                    undoWriter, 
                    this.Dbms, 
                    this.ChangeLogTableName, 
                    this.Delimiter, 
                    this.DelimiterType, 
                    this.TemplateDir);
            }

            try
            {
                Controller controller = new Controller(
                    changeScriptRepository, 
                    databaseSchemaVersionManager, 
                    doScriptApplier, 
                    undoScriptApplier, 
                    this.InfoWriter);

                controller.ProcessChangeScripts(this.LastChangeToApply);

                queryExecuter.Close();
            }
            finally
            {
                if (doWriter != null)
                    doWriter.Dispose();

                if (undoWriter != null)
                    undoWriter.Dispose();
            }
        }

        private void Validate() 
        {
            this.CheckForRequiredParameter(this.Dbms, "dbms");
            this.CheckForRequiredParameter(this.ConnectionString, "connectionString");
            this.CheckForRequiredParameter(this.ScriptDirectory, "dir");
            this.CheckForRequiredParameter(this.InfoWriter, "infoWriter");

            if (this.ScriptDirectory == null || !this.ScriptDirectory.Exists) 
            {
                throw new UsageException("Script directory must point to a valid directory");
            }
        }

        private void CheckForRequiredParameter(string parameterValue, string parameterName) 
        {
            if (string.IsNullOrEmpty(parameterValue))
            {
                UsageException.ThrowForMissingRequiredValue(parameterName);
            }
        }

        private void CheckForRequiredParameter(object parameterValue, string parameterName) 
        {
            if (parameterValue == null) 
            {
                UsageException.ThrowForMissingRequiredValue(parameterName);
            }
        }
    }
}
