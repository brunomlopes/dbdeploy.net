using System.Collections.Generic;
using System.Linq;

namespace Net.Sf.Dbdeploy
{
    using System;
    using System.IO;
    using System.Reflection;
    using Appliers;
    using Configuration;
    using Database;
    using Exceptions;
    using Scripts;

    /// <summary>
    /// Main class for running database deployment.
    /// </summary>
    public class DbDeployer
    {
        /// <summary>
        /// Generates the welcome string.
        /// </summary>
        /// <returns>Welcome message.</returns>
        public string GenerateWelcomeString()
        {
            Version version = Assembly.GetAssembly(this.GetType()).GetName().Version;
            
            return "dbdeploy.net " + version;
        }

        /// <summary>
        /// Executes the a database deployment with the specified config.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="infoWriter">The info writer.</param>
        /// <exception cref="System.InvalidOperationException">SQLCMD mode can only be applied against an mssql database.</exception>
        public void Execute(DbDeployConfig config, TextWriter infoWriter)
        {
            this.Validate(config, infoWriter);

            infoWriter.WriteLine();
            infoWriter.WriteLine("==========================================================");
            infoWriter.WriteLine(this.GenerateWelcomeString());

            var factory = new DbmsFactory(config.Dbms, config.ConnectionString, config.DllPathConnector);
            
            var dbmsSyntax = factory.CreateDbmsSyntax();
            dbmsSyntax.SetDefaultDatabaseName(config.ConnectionString);

            var queryExecuter = new QueryExecuter(factory);
            var databaseSchemaVersionManager = new DatabaseSchemaVersionManager(queryExecuter, dbmsSyntax, config.ChangeLogTableName);

            var changeScriptRepositoryFactory = new ChangeScriptRepositoryFactory(config, infoWriter);
            var changeScriptRepository = changeScriptRepositoryFactory.Obter();

            IChangeScriptApplier doScriptApplier;
            TextWriter doWriter = null;
            QueryExecuter applierExecutor = null;

            if (config.OutputFile != null) 
            {
                doWriter = new StreamWriter(config.OutputFile.OpenWrite(), config.Encoding);

                doScriptApplier = new TemplateBasedApplier(
                    doWriter,
                    dbmsSyntax,
                    config.ChangeLogTableName,
                    config.Delimiter,
                    config.DelimiterType,
                    config.TemplateDirectory);
            }
            else if (config.UseSqlCmd)
            {
                // Verify database is MSSQL.
                if (!string.Equals(config.Dbms, SupportedDbms.MSSQL, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new InvalidOperationException("SQLCMD mode can only be applied against an mssql database.");
                }

                doScriptApplier = new SqlCmdApplier(
                    config.ConnectionString,
                    databaseSchemaVersionManager,
                    dbmsSyntax,
                    config.ChangeLogTableName,
                    infoWriter);
            }
            else 
            {
                var splitter = new QueryStatementSplitter
                {
                    Delimiter = config.Delimiter,
                    DelimiterType = config.DelimiterType,
                    LineEnding = config.LineEnding,
                };

                // Do not share query executor between schema manager and applier, since a failure in one will effect the other.
                applierExecutor = new QueryExecuter(factory);
                doScriptApplier = new DirectToDbApplier(
                    applierExecutor, 
                    databaseSchemaVersionManager, 
                    splitter, 
                    dbmsSyntax, 
                    config.ChangeLogTableName,
                    infoWriter);
            }

            IChangeScriptApplier undoScriptApplier = null;
            TextWriter undoWriter = null;

            if (config.UndoOutputFile != null) 
            {
                undoWriter = new StreamWriter(config.UndoOutputFile.OpenWrite(), config.Encoding);

                undoScriptApplier = new UndoTemplateBasedApplier(
                    undoWriter,
                    dbmsSyntax,
                    config.ChangeLogTableName,
                    config.Delimiter,
                    config.DelimiterType,
                    config.TemplateDirectory);
            }

            try
            {
                var controller = new Controller(
                    changeScriptRepository, 
                    databaseSchemaVersionManager, 
                    doScriptApplier, 
                    undoScriptApplier, 
                    config.AutoCreateChangeLogTable,
                    infoWriter);

                controller.ProcessChangeScripts(config.LastChangeToApply, config.ForceUpdate);

                queryExecuter.Close();

                if (applierExecutor != null)
                {
                    applierExecutor.Close();
                }
            }
            finally
            {
                if (doWriter != null)
                {
                    doWriter.Dispose();
                }

                if (undoWriter != null)
                {
                    undoWriter.Dispose();
                }
            }
        }

        /// <summary>
        /// Validates the specified config.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="infoWriter">The info writer.</param>
        /// <exception cref="UsageException">Script directory must point to a valid directory</exception>
        private void Validate(DbDeployConfig config, TextWriter infoWriter)
        {
            this.CheckForRequiredParameter(config.Dbms, "dbms");
            this.CheckForRequiredParameter(config.ConnectionString, "connectionString");
            this.CheckForRequiredParameter(infoWriter, "infoWriter");

            if (config.ScriptAssembly == null)
            {
                this.CheckForRequiredParameter(config.ScriptDirectory, "dir");
            }
            
            if (config.ScriptDirectory != null && !config.ScriptDirectory.Exists)
            {
                throw new UsageException(string.Format("The directory '{0}' does not exist.\nScript directory must point to a valid directory", config.ScriptDirectory));
            }
        }

        /// <summary>
        /// Checks for required parameter.
        /// </summary>
        /// <param name="parameterValue">The parameter value.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        private void CheckForRequiredParameter(string parameterValue, string parameterName) 
        {
            if (string.IsNullOrEmpty(parameterValue))
            {
                UsageException.ThrowForMissingRequiredValue(parameterName);
            }
        }

        /// <summary>
        /// Checks for required parameter.
        /// </summary>
        /// <param name="parameterValue">The parameter value.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        private void CheckForRequiredParameter(object parameterValue, string parameterName) 
        {
            if (parameterValue == null) 
            {
                UsageException.ThrowForMissingRequiredValue(parameterName);
            }
        }
    }
}
