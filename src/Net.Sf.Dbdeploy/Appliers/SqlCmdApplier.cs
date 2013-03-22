namespace Net.Sf.Dbdeploy.Appliers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Net.Sf.Dbdeploy.Database;
    using Net.Sf.Dbdeploy.Database.SqlCmd;
    using Net.Sf.Dbdeploy.Exceptions;
    using Net.Sf.Dbdeploy.Scripts;

    /// <summary>
    /// Applier for running scripts using SQLCMD mode against MSSQL.
    /// </summary>
    public class SqlCmdApplier : IChangeScriptApplier
    {
        /// <summary>
        /// The database connection string.
        /// </summary>
        private readonly string connectionString;

        /// <summary>
        /// The info text writer to display output information.
        /// </summary>
        private readonly TextWriter infoTextWriter;

        /// <summary>
        /// The schema version manager for tracking change scripts.
        /// </summary>
        private readonly DatabaseSchemaVersionManager schemaVersionManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlCmdApplier" /> class.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="schemaVersionManager">The schema version manager.</param>
        /// <param name="infoTextWriter">The info text writer.</param>
        public SqlCmdApplier(string connectionString, DatabaseSchemaVersionManager schemaVersionManager, TextWriter infoTextWriter)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            if (schemaVersionManager == null)
            {
                throw new ArgumentNullException("schemaVersionManager");
            }

            if (infoTextWriter == null)
            {
                throw new ArgumentNullException("infoTextWriter");
            }

            this.schemaVersionManager = schemaVersionManager;
            this.infoTextWriter = infoTextWriter;
            this.connectionString = connectionString;
        }

        /// <summary>
        /// Applies the specified change scripts.
        /// </summary>
        /// <param name="changeScripts">The change scripts.</param>
        public void Apply(IEnumerable<ChangeScript> changeScripts)
        {
            this.infoTextWriter.WriteLine(changeScripts.Any() ? "Applying change scripts...\n" : "No changes to apply.\n");

            using (var sqlCmdExecutor = new SqlCmdExecutor(this.connectionString))
            {
                foreach (var script in changeScripts)
                {
                    this.schemaVersionManager.RecordScriptStatus(script, ScriptStatus.Started);

                    this.infoTextWriter.WriteLine(script);
                    this.infoTextWriter.WriteLine("----------------------------------------------------------");
                    var output = new StringBuilder();

                    var success = false;
                    try
                    {
                        success = sqlCmdExecutor.ExecuteFile(script.FileInfo, output);
                        if (!success)
                        {
                            throw new DbDeployException(string.Format(CultureInfo.InvariantCulture, "Script '{0}' failed.", script.FileName)); 
                        }
                    }
                    finally 
                    {
                        this.infoTextWriter.WriteLine(output);
                        this.schemaVersionManager.RecordScriptStatus(script, success ? ScriptStatus.Success : ScriptStatus.Failure, output.ToString());
                    }
                }
            }
        }
    }
}
