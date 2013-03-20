using System.Collections.Generic;

namespace Net.Sf.Dbdeploy
{
    using Net.Sf.Dbdeploy.Database;

    public interface IAppliedChangesProvider
    {
        ICollection<ChangeEntry> GetAppliedChanges();

        /// <summary>
        /// Gets or sets a value indicating whether the change log table should be created if it does not exist.
        /// </summary>
        /// <value>
        /// <c>true</c> if auto create change log table; otherwise, <c>false</c>.
        /// </value>
        bool AutoCreateChangeLogTable { get; set; }
    }
}
