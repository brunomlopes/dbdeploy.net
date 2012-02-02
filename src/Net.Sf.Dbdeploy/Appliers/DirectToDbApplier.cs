using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using Net.Sf.Dbdeploy.Database;
using Net.Sf.Dbdeploy.Exceptions;
using Net.Sf.Dbdeploy.Scripts;

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
            this.infoTextWriter = infoTextWriter;
            this.queryExecuter = queryExecuter;
            this.schemaVersionManager = schemaVersionManager;
            this.splitter = splitter;
        }

        public void Apply(IEnumerable<ChangeScript> changeScripts)
        {
            foreach (var script in changeScripts)
            {
                // Begin transaction
                this.queryExecuter.BeginTransaction();

                this.infoTextWriter.WriteLine("Applying " + script + "...");

                // Apply changes and update changelog table
                this.ApplyChangeScript(script);
                this.InsertToSchemaVersionTable(script);

                // Commit transaction
                this.queryExecuter.CommitTransaction();
            }
        }

        protected void ApplyChangeScript(ChangeScript script)
        {
            IList<string> statements = new List<string>(this.splitter.Split(script.GetContent()));

            int i = 0;

            foreach (var statement in statements)
            {
                try
                {
                    if (statements.Count > 1)
                    {
                        this.infoTextWriter.WriteLine(" -> statement " + (i + 1) + " of " + statements.Count + "...");
                    }

                    this.queryExecuter.Execute(statement);

                    i++;
                }
                catch (DbException e)
                {
                    throw new ChangeScriptFailedException(e, script, i + 1, statement);
                }
            }
        }

        protected void InsertToSchemaVersionTable(ChangeScript changeScript) 
        {
            this.schemaVersionManager.RecordScriptApplied(changeScript);
        }
    }
}
