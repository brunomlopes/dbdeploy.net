namespace Net.Sf.Dbdeploy.Database
{
    /// <summary>
    /// Interface for Database Management System syntax.
    /// </summary>
    public interface IDbmsSyntax
    {
        /// <summary>
        /// Gets the syntax to get the current timestamp.
        /// </summary>
        /// <value>
        /// The current timestamp syntax.
        /// </value>
        string CurrentTimestamp { get; }

        /// <summary>
        /// Gets the syntax to get the current user.
        /// </summary>
        /// <value>
        /// The current user syntax.
        /// </value>
        string CurrentUser { get; }

        /// <summary>
        /// Gets the Change Log Table create script.
        /// </summary>
        /// <param name="tableName">Name of the change log table.</param>
        /// <returns>
        /// Script to create a Change Log table for the current DBMS.
        /// </returns>
        string CreateChangeLogTable(string tableName);
    }
}