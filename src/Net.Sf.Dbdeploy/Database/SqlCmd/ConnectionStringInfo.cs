namespace Net.Sf.Dbdeploy.Database.SqlCmd
{
    /// <summary>
    /// Connection string parsed information.
    /// </summary>
    public class ConnectionStringInfo
    {
        /// <summary>
        /// Gets or sets the database server name.
        /// </summary>
        /// <value>
        /// The server.
        /// </value>
        public string Server { get; set; }

        /// <summary>
        /// Gets or sets the database name.
        /// </summary>
        /// <value>
        /// The database.
        /// </value>
        public string Database { get; set; }

        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        /// <value>
        /// The user id.
        /// </value>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the user password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use trusted connection.
        /// </summary>
        /// <value>
        ///   <c>true</c> if use trusted connection; otherwise, <c>false</c>.
        /// </value>
        public bool TrustedConnection { get; set; }
    }
}
