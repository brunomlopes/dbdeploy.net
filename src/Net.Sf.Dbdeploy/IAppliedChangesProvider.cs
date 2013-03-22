namespace Net.Sf.Dbdeploy
{
    using System.Collections.Generic;

    using Net.Sf.Dbdeploy.Database;

    /// <summary>
    /// Interface for provider to retrieve changes that have been applied to the database.
    /// </summary>
    public interface IAppliedChangesProvider
    {
        /// <summary>
        /// Gets or sets a value indicating whether the change log table should be created if it does not exist.
        /// </summary>
        /// <value>
        /// <c>true</c> if auto create change log table; otherwise, <c>false</c>.
        /// </value>
        bool AutoCreateChangeLogTable { get; set; }

        /// <summary>
        /// Gets the applied changes to the database.
        /// </summary>
        /// <returns>List of changes applied.</returns>
        IList<ChangeEntry> GetAppliedChanges();
    }
}
