namespace Net.Sf.Dbdeploy.Database
{
    public interface IDbmsSyntax
    {
        string GenerateTimestamp();

        string GenerateUser();

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