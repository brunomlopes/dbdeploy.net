namespace Net.Sf.Dbdeploy.Database
{
    /// <summary>
    /// Represents database table information.
    /// </summary>
    public class TableInfo
    {
        /// <summary>
        /// Gets or sets the schema for the table.
        /// </summary>
        /// <value>
        /// The schema.
        /// </value>
        public string Schema { get; set; }

        /// <summary>
        /// Gets or sets the name of the table.
        /// </summary>
        /// <value>
        /// The name of the table.
        /// </value>
        public string TableName { get; set; }
    }
}
