using Net.Sf.Dbdeploy.Configuration;

namespace Net.Sf.Dbdeploy.Database
{
    public class PostgreDbmsSyntax : DbmsSyntax
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreDbmsSyntax" /> class.
        /// </summary>
        public PostgreDbmsSyntax() : base(SupportedDbms.POSTGRE)
        {}

        /// <summary>
        /// Gets the get timestamp.
        /// </summary>
        /// <value>
        /// The get timestamp.
        /// </value>
        public override string CurrentTimestamp
        {
            get { return "CURRENT_DATE"; }
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