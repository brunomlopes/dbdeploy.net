namespace Net.Sf.Dbdeploy.Database
{
    /// <summary>
    /// Syntax for MySQL.
    /// </summary>
    public class MySqlDbmsSyntax : DbmsSyntax
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlDbmsSyntax" /> class.
        /// </summary>
        public MySqlDbmsSyntax()
            : base("mysql")
        {
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
            get { return "USER()"; }
        }
    }
}