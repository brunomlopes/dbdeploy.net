using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using Net.Sf.Dbdeploy;
using Net.Sf.Dbdeploy.Configuration;
using Net.Sf.Dbdeploy.Database;
using Net.Sf.Dbdeploy.Scripts;

namespace Dbdeploy.Powershell.Commands
{
    internal class MyClass
    {
        
    }

    public class XmlConfiguration : IConfiguration
    {
        private Dictionary<string, string> keys;

        public XmlConfiguration(string xmlFilePath)
        {
            using(var file = new StreamReader(File.OpenRead(xmlFilePath)))
            {
                LoadXmlFile(file);
            }
        }

        public XmlConfiguration(TextReader xmlFile)
        {
            LoadXmlFile(xmlFile);
        }

        private void LoadXmlFile(TextReader xmlFile)
        {
            XDocument doc = XDocument.Load(xmlFile);
            this.keys = doc.XPathSelectElements("/configuration/appSettings/add")
                .ToDictionary(n => n.Attribute("key").Value, n => n.Attribute("value").Value);

            DbConnectionString = ValueOrException("db.connection", "Connection string");
            DbType = ValueOrException("db.type", "Database type");
            DbDeltaSet = ValueOrException("db.deltaSet", "Delta set name");
            TableName = ValueOrException("db.tableName", "Table name for change log");
            UseTransaction = ValueOrDefault("db.useTransaction", false);
        }

        private bool ValueOrDefault(string key, bool defaultValue)
        {
            return keys.ContainsKey(key) ? keys[key].Trim().ToLowerInvariant() == "true" : defaultValue;
        }

        private string ValueOrException(string key, string message)
        {
            if (!keys.ContainsKey(key))
            {
                throw new InvalidOperationException(string.Format("Missing key '{0}' ({1})", key, message));
            }
            return keys[key];
        }

        public string DbConnectionString { get; private set; }
        public string DbType { get; private set; }
        public string DbDeltaSet { get; private set; }
        public bool UseTransaction { get; private set; }
        public int? CurrentDbVersion { get; private set; }
        public string TableName { get; private set; }
    }

    [Cmdlet(VerbsCommon.Select, "DbUpdate")]
    public class SelectDbUpdate : PSCmdlet
    {
        public SelectDbUpdate()
        {
            DeltasDirectory = ".";
        }

        protected override void ProcessRecord()
        {
            var configurationFile = ToAbsolutePath(ConfigurationFile);
            var deltasDirectory = ToAbsolutePath(DeltasDirectory);


            IConfiguration config = new XmlConfiguration(configurationFile);
            DbmsFactory factory = new DbmsFactory(config.DbType, config.DbConnectionString);
            DatabaseSchemaVersionManager databaseSchemaVersion = new DatabaseSchemaVersionManager(factory,
                                                                                                  config.DbDeltaSet,
                                                                                                  config.CurrentDbVersion,
                                                                                                  config.TableName);

            TextWriter outputTextWriter;
            if (string.IsNullOrEmpty(OutputFile))
            {
                outputTextWriter = new PowershellTextWriter(this);
            }
            else
            {
                outputTextWriter = new StreamWriter(File.OpenWrite(ToAbsolutePath(OutputFile)), Encoding.UTF8);
            } 
            
            TextWriter undoTextWriter = null;
            if (!string.IsNullOrEmpty(UndoOutputFile))
            {
                undoTextWriter = new StreamWriter(File.OpenWrite(ToAbsolutePath(UndoOutputFile)), Encoding.UTF8);
            }

            WriteVerbose("dbdeploy v2.12");

            new PowershellPrintStreamDeployer(databaseSchemaVersion, new DirectoryInfo(deltasDirectory), outputTextWriter,
                                      factory.CreateDbmsSyntax(), config.UseTransaction, undoTextWriter).DoDeploy(Int32.MaxValue);

            

        }

        private string ToAbsolutePath(string deltasDirectory)
        {
            if (!Path.IsPathRooted(deltasDirectory))
            {
                deltasDirectory = Path.Combine(SessionState.Path.CurrentFileSystemLocation.Path, deltasDirectory);
            }
            return deltasDirectory;
        }

        [Parameter(Mandatory = true, Position = 0)]
        public string ConfigurationFile { get; set; }

        [Parameter(Position = 1)]
        public string DeltasDirectory { get; set; }

        [Parameter]
        public string OutputFile { get; set; }

        [Parameter]
        public string UndoOutputFile { get; set; }
    }


    public class PowershellPrintStreamDeployer
    {
        private readonly DirectoryInfo dir;
        private readonly TextWriter doOutputPrintStream;
        private readonly TextWriter undoOutputPrintStream;
        private readonly DatabaseSchemaVersionManager schemaManager;
        private readonly IDbmsSyntax dbmsSyntax;
        private readonly bool useTransaction;

        public PowershellPrintStreamDeployer(DatabaseSchemaVersionManager schemaManager, DirectoryInfo dir,
                                     TextWriter outputPrintStream,
                                     IDbmsSyntax dbmsSyntax, bool useTransaction, TextWriter undoOutputPrintStream)
        {   
            this.schemaManager = schemaManager;
            this.dir = dir;
            this.doOutputPrintStream = outputPrintStream;
            this.dbmsSyntax = dbmsSyntax;
            this.useTransaction = useTransaction;
            this.undoOutputPrintStream = undoOutputPrintStream;
        }

        public void DoDeploy(int lastChangeToApply)
        {
            List<ChangeScript> changeScripts = new DirectoryScanner().GetChangeScriptsForDirectory(dir);
            ChangeScriptRepository repository = new ChangeScriptRepository(changeScripts);
            List<int> appliedChanges = schemaManager.GetAppliedChangeNumbers();

            GenerateChangeScripts(repository, lastChangeToApply, appliedChanges);
            if (undoOutputPrintStream != null)
            {
                GenerateUndoChangeScripts(repository, lastChangeToApply, appliedChanges);
            }
        }

        private void GenerateChangeScripts(ChangeScriptRepository repository, int lastChangeToApply, List<int> appliedChanges)
        {
            ChangeScriptExecuter doScriptExecuter = new ChangeScriptExecuter(doOutputPrintStream, dbmsSyntax, useTransaction);
            Controller doController = new Controller(schemaManager, repository, doScriptExecuter);
            doController.ProcessDoChangeScripts(lastChangeToApply, appliedChanges);
            doOutputPrintStream.Flush();
        }

        private void GenerateUndoChangeScripts(ChangeScriptRepository repository, int lastChangeToApply, List<int> appliedChanges)
        {
            ChangeScriptExecuter undoScriptExecuter = new ChangeScriptExecuter(undoOutputPrintStream, dbmsSyntax, useTransaction);
            Controller undoController = new Controller(schemaManager, repository, undoScriptExecuter);
            undoController.ProcessUndoChangeScripts(lastChangeToApply, appliedChanges);
            undoOutputPrintStream.Flush();
        }
    }
}
