using System.Collections.Generic;

using Net.Sf.Dbdeploy.Scripts;
using System;
using System.IO;

using Net.Sf.Dbdeploy.Database;

namespace Net.Sf.Dbdeploy.Appliers
{
    using System.Data.Common;
    using System.Globalization;

    using Net.Sf.Dbdeploy.Database.SqlCmd;
    using Net.Sf.Dbdeploy.Exceptions;

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
            using (var sqlCmdExecutor = new SqlCmdExecutor(this.connectionString, this.infoTextWriter))
            {
                foreach (var script in changeScripts)
                {
                    this.infoTextWriter.WriteLine("Applying " + script + "...");
                    var success = sqlCmdExecutor.ExecuteFile(script.GetFile());
                    if (success)
                    {
                        this.schemaVersionManager.RecordScriptApplied(script);
                    }
                    else
                    {
                        throw new DbDeployException(string.Format(CultureInfo.InvariantCulture, "Script '{0}' failed.", script.GetDescription()));                        
                    }
                }
            }
        }
    }
}
