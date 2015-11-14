namespace Net.Sf.Dbdeploy.Database
{
    public class PostgresqlDbmsSyntax : DbmsSyntax
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgresqlDbmsSyntax" /> class.
        /// </summary>
        public PostgresqlDbmsSyntax() : base("postgresql")
        {}

        public override string TableExists(string tableName)
        {
            return base.TableExists(tableName.ToLowerInvariant());
        }

        /// <summary>
        /// Gets the get timestamp.
        /// </summary>
        /// <value>
        /// The get timestamp.
        /// </value>
        public override string CurrentTimestamp
        {
            get { return "CURRENT_TIMESTAMP"; }
        }

        /// <summary>
        /// Gets the syntax to get the current user.
        /// </summary>
        /// <value>
        /// The current user syntax.
        /// </value>
        public override string CurrentUser
        {
            get { return "CURRENT_USER"; }
        }
    }
}