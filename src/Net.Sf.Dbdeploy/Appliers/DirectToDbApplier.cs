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

        /// <summary>
        /// Aplicar o ChangeScript
        /// </summary>
        /// <param name="changeScript">The changeScript.</param>
        /// <param name="createChangeLogTable">Create or not ChangeLog table</param>
        public void ApplyChangeScript(ChangeScript changeScript, bool createChangeLogTable)
        {
            Apply(new List<ChangeScript> { changeScript }, createChangeLogTable);
        }

        public void Apply(IEnumerable<ChangeScript> changeScripts, bool createChangeLogTable)
        {
            if (createChangeLogTable)
                CriarTabelaChangeLog();

            infoTextWriter.WriteLine(changeScripts.Any() ? "Applying change scripts...\n" : "No changes to apply.\n");

            foreach (var script in changeScripts)
            {
                ApplyScript(script, script.GetContent(), ScriptStatus.Success);
            }
        }

        /// <summary>
        /// Aplicar o conteúdo do script
        /// </summary>
        /// <param name="changeScript"></param>
        /// <param name="scriptContent"></param>
        /// <param name="createChangeLogTable"></param>
        public void ApplyScriptContent(ChangeScript changeScript, string scriptContent, bool createChangeLogTable)
        {
            if (createChangeLogTable)
                CriarTabelaChangeLog();

            ApplyScript(changeScript, scriptContent, ScriptStatus.SucessRevisedUser);
        }

        private void ApplyScript(ChangeScript script, string scriptContent, ScriptStatus scriptStatusSucess)
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
                ApplyChangeScript(script, scriptContent, output);
                RecordScriptStatus(script, scriptStatusSucess, output.ToString());
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    output.AppendLine(ex.Message);
                }

                RecordScriptStatus(script, ScriptStatus.Failure, output.ToString());
                throw;
            }
            finally
            {
                // Commit transaction
                queryExecuter.CommitTransaction();
            }
        }

        /// <summary>
        /// Applies the change changeScript.
        /// </summary>
        /// <param name="changeScript">The changeScript.</param>
        /// <param name="scriptContent"></param>
        /// <param name="output">The output from applying the change changeScript.</param>
        protected void ApplyChangeScript(ChangeScript changeScript, string scriptContent, StringBuilder output)
        {
            var statements = splitter.Split(scriptContent);

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
