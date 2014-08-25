using System.Collections.Generic;
using Net.Sf.Dbdeploy.Exceptions;
using Net.Sf.Dbdeploy.Scripts;

namespace Net.Sf.Dbdeploy.Database
{
    /// <summary>
    /// Interface for provider to retrieve changes that have been applied to the database.
    /// </summary>
    public interface IDatabaseSchemaVersionManager
    {
        /// <summary>
        /// Gets the applied changes to the database.
        /// </summary>
        /// <returns>List of changes applied.</returns>
        IList<ChangeEntry> GetAppliedChanges();

        /// <summary>
        /// Verifies the change log table exists.
        /// </summary>
        /// <exception cref="ChangelogTableDoesNotExistException">Thrown when the change log table is not found.</exception>
        bool ChangeLogTableExists();

        /// <summary>
        /// Record status of script on ChangeLog table
        /// </summary>
        /// <param name="script"></param>
        /// <param name="status"></param>
        /// <param name="output"></param>
        void RecordScriptStatus(ChangeScript script, ScriptStatus status, string output = null);
    }
}
