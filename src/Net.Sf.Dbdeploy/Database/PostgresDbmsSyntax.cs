namespace Net.Sf.Dbdeploy.Database
{
    /// <summary>
    /// Postgres syntax.
    /// </summary>
    public class PostgresDbmsSyntax : DbmsSyntax
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgresDbmsSyntax" /> class.
        /// </summary>
        public PostgresDbmsSyntax()
            : base("postgres")
        {
        }

        public override string DefaultSchema
        {
            get { return "public"; }
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
            get { return "USER"; }
        }
    }
}