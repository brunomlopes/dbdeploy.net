namespace Net.Sf.Dbdeploy.Appliers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Database;
    using Exceptions;
    using Scripts;

    public class DirectToDbApplier : IChangeScriptApplier
    {
        private readonly QueryExecuter queryExecuter;

        private readonly IDatabaseSchemaVersionManager schemaVersionManager;

        private readonly QueryStatementSplitter splitter;

        private readonly IDbmsSyntax dbmsSyntax;

        private readonly string changeLogTableName;

        private readonly TextWriter infoTextWriter;

        public DirectToDbApplier(
            QueryExecuter queryExecuter,
            IDatabaseSchemaVersionManager schemaVersionManager,
            QueryStatementSplitter splitter,
            IDbmsSyntax dbmsSyntax,
            string changeLogTableName,
            TextWriter infoTextWriter)
        {
            if (queryExecuter == null)
                throw new ArgumentNullException("queryExecuter");

            if (schemaVersionManager == null)
                throw new ArgumentNullException("schemaVersionManager");

            if (splitter == null)
                throw new ArgumentNullException("splitter");

            if (infoTextWriter == null)
                throw new ArgumentNullException("infoTextWriter");

            this.queryExecuter = queryExecuter;
            this.schemaVersionManager = schemaVersionManager;
            this.splitter = splitter;
            this.dbmsSyntax = dbmsSyntax;
            this.changeLogTableName = changeLogTableName;
            this.infoTextWriter = infoTextWriter;
        }

        public void Apply(IEnumerable<ChangeScript> changeScripts, bool createChangeLogTable)
        {
            if (createChangeLogTable)
                CriarTabelaChangeLog();

            infoTextWriter.WriteLine(changeScripts.Any() ? "Applying change scripts...\n" : "No changes to apply.\n");

            foreach (var script in changeScripts)
            {
                RecordScriptStatus(script, ScriptStatus.Started);

                // Begin transaction
                queryExecuter.BeginTransaction();

                infoTextWriter.WriteLine(script);
                infoTextWriter.WriteLine("----------------------------------------------------------");

                // Apply changes and update ChangeLog table
                var output = new StringBuilder();
                try
                {
                    ApplyChangeScript(script, output);
                    RecordScriptStatus(script, ScriptStatus.Success, output.ToString());
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                    {
                        output.AppendLine(ex.InnerException.Message);
                    }

                    RecordScriptStatus(script, ScriptStatus.Failure, output.ToString());
                    throw;
                }

                // Commit transaction
                queryExecuter.CommitTransaction();
            }
        }

        /// <summary>
        /// Applies the change changeScript.
        /// </summary>
        /// <param name="changeScript">The changeScript.</param>
        /// <param name="output">The output from applying the change changeScript.</param>
        protected void ApplyChangeScript(ChangeScript changeScript, StringBuilder output)
        {
            var statements = splitter.Split(changeScript.GetContent());

            var i = 0;

            foreach (var statement in statements)
            {
                try
                {
                    if (statements.Count > 1)
                    {
                        infoTextWriter.WriteLine(" -> statement " + (i + 1) + " of " + statements.Count + "...");
                    }

                    queryExecuter.Execute(statement, output);

                    i++;
                }
                catch (DbException e)
                {
                    throw new ChangeScriptFailedException(e, changeScript, i + 1, statement);
                }
                finally
                {
                    // Write out SQL execution output.
                    if (output.Length > 0)
                    {
                        infoTextWriter.WriteLine(output.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Aplicar um script por vez, implementação para o Inventti.Config
        /// </summary>
        /// <param name="changeScript">The changeScript.</param>
        /// <param name="createChangeLogTable">Create or not ChangeLog table</param>
        public void ApplyChangeScript(ChangeScript changeScript, bool createChangeLogTable)
        {
            if (createChangeLogTable)
                CriarTabelaChangeLog();

            schemaVersionManager.RecordScriptStatus(changeScript, ScriptStatus.Started);
            queryExecuter.BeginTransaction();

            var statements = splitter.Split(changeScript.GetContent());

            var i = 0;

            foreach (var statement in statements)
            {
                var output = new StringBuilder();
                try
                {
                    if (statements.Count > 1)
                    {
                        infoTextWriter.WriteLine(" -> statement " + (i + 1) + " of " + statements.Count + "...");
                    }

                    queryExecuter.Execute(statement, output);

                    i++;
                }
                catch (Exception exception)
                {
                    if (exception.InnerException != null)
                        output.AppendLine(exception.InnerException.Message);

                    schemaVersionManager.RecordScriptStatus(changeScript, ScriptStatus.Failure, output.ToString());
                    throw;
                }
                finally
                {
                    // Write out SQL execution output.
                    if (output.Length > 0)
                    {
                        infoTextWriter.WriteLine(output.ToString());
                    }
                }
            }
            queryExecuter.CommitTransaction();
            schemaVersionManager.RecordScriptStatus(changeScript, ScriptStatus.Success);
        }

        protected void ApplyChangeScript(string script)
        {
            var statements = splitter.Split(script);
            var i = 0;

            foreach (var statement in statements)
            {
                try
                {
                    if (statements.Count > 1)
                    {
                        infoTextWriter.WriteLine(" -> statement " + (i + 1) + " of " + statements.Count + "...");
                    }

                    queryExecuter.Execute(statement);

                    i++;
                }
                catch (DbException e)
                {
                    throw new ScriptFailedException(e, script, i + 1, statement);
                }
            }
            
        }

        /// <summary>
        /// Records details about a change changeScript in the database.
        /// </summary>
        /// <param name="changeScript">The change changeScript.</param>
        /// <param name="status">Status of the changeScript execution.</param>
        /// <param name="output">The output from running the changeScript.</param>
        protected void RecordScriptStatus(ChangeScript changeScript, ScriptStatus status, string output = null) 
        {
            schemaVersionManager.RecordScriptStatus(changeScript, status, output);
        }

        private void CriarTabelaChangeLog()
        {
            infoTextWriter.WriteLine("Creating change log table");
            var scriptCreateChangeLog = dbmsSyntax.CreateChangeLogTableSqlScript(changeLogTableName);
            ApplyChangeScript(scriptCreateChangeLog);
        }
    }
}
