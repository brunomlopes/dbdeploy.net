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
            foreach (var script in changeScripts)
            {
                // Begin transaction
                this.queryExecuter.BeginTransaction();

                this.infoTextWriter.WriteLine("Applying " + script + "...");

                // Apply changes and update ChangeLog table
                var output = this.ApplyChangeScript(script);
                this.RecordScriptStatus(script, ScriptStatus.Success, output);

                // Commit transaction
                this.queryExecuter.CommitTransaction();
            }
        }

        /// <summary>
        /// Applies the change script.
        /// </summary>
        /// <param name="script">The script.</param>
        /// <returns>The output from applying the change script.</returns>
        protected string ApplyChangeScript(ChangeScript script)
        {
            var output = new StringBuilder();
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

            return output.ToString();
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
