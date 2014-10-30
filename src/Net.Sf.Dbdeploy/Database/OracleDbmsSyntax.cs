using Net.Sf.Dbdeploy.Configuration;

namespace Net.Sf.Dbdeploy.Database
{
    /// <summary>
    /// Oracle syntax.
    /// </summary>
    public class OracleDbmsSyntax : DbmsSyntax
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OracleDbmsSyntax" /> class.
        /// </summary>
        public OracleDbmsSyntax()
            : base(SupportedDbms.ORACLE)
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
            get { return "USER"; }
        }

        protected override string GetQueryTableExists(TableInfo tableInfo)
        {
            return string.Format("SELECT * FROM USER_TABLES WHERE TABLE_NAME = '{0}'", tableInfo.TableName.ToUpper());
        }
    }
}