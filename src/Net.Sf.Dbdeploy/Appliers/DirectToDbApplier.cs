using Net.Sf.Dbdeploy.Database;
using Net.Sf.Dbdeploy.Exceptions;
using Net.Sf.Dbdeploy.Scripts;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Text;

namespace Net.Sf.Dbdeploy.Appliers
{
    using System.Linq;

    public class DirectToDbApplier : IChangeScriptApplier
    {
        private readonly QueryExecuter queryExecuter;

        private readonly DatabaseSchemaVersionManager schemaVersionManager;

        private readonly QueryStatementSplitter splitter;

        private readonly TextWriter infoTextWriter;

        public DirectToDbApplier(
            QueryExecuter queryExecuter,
            DatabaseSchemaVersionManager schemaVersionManager,
            QueryStatementSplitter splitter,
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
            this.infoTextWriter = infoTextWriter;
        }

        public void Apply(IEnumerable<ChangeScript> changeScripts)
        {
            this.infoTextWriter.WriteLine(changeScripts.Any() ? "Applying change scripts...\n" : "No changes to apply.\n");

            foreach (var script in changeScripts)
            {
                // Begin transaction
                this.queryExecuter.BeginTransaction();

                this.infoTextWriter.WriteLine(script);
                this.infoTextWriter.WriteLine("----------------------------------------------------------");

                // Apply changes and update ChangeLog table
                var output = new StringBuilder();
                try
                {
                    this.ApplyChangeScript(script, output);
                    this.RecordScriptStatus(script, ScriptStatus.Success, output.ToString());
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                    {
                        output.AppendLine(ex.InnerException.Message);
                    }

                    this.RecordScriptStatus(script, ScriptStatus.Failure, output.ToString());
                    throw;
                }

                // Commit transaction
                this.queryExecuter.CommitTransaction();
            }
        }

        /// <summary>
        /// Applies the change script.
        /// </summary>
        /// <param name="script">The script.</param>
        /// <param name="output">The output from applying the change script.</param>
        protected void ApplyChangeScript(ChangeScript script, StringBuilder output)
        {
            ICollection<string> statements = this.splitter.Split(script.GetContent());

            int i = 0;

            foreach (var statement in statements)
            {
                try
                {
                    if (statements.Count > 1)
                    {
                        this.infoTextWriter.WriteLine(" -> statement " + (i + 1) + " of " + statements.Count + "...");
                    }

                    this.queryExecuter.Execute(statement, output);

                    i++;
                }
                catch (DbException e)
                {
                    throw new ChangeScriptFailedException(e, script, i + 1, statement);
                }
                finally
                {
                    // Write out SQL execution output.
                    if (output.Length > 0)
                    {
                        this.infoTextWriter.WriteLine(output.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Records details about a change script in the database.
        /// </summary>
        /// <param name="changeScript">The change script.</param>
        /// <param name="status">Status of the script execution.</param>
        /// <param name="output">The output from running the script.</param>
        protected void RecordScriptStatus(ChangeScript changeScript, ScriptStatus status, string output) 
        {
            this.schemaVersionManager.RecordScriptStatus(changeScript, status, output);
        }
    }
}
