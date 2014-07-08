using System.Globalization;
using Net.Sf.Dbdeploy.Database.SqlCmd;

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

        /// <summary>
        /// Set default database name MySQL specialization
        /// </summary>
        /// <param name="connectionString"></param>
        public override void SetDefaultDatabaseName(string connectionString)
        {
            DatabaseName = ConnectionStringParser.Parse(connectionString).Database;
        }

        protected override string GetQueryTableExists(TableInfo tableInfo)
        {
            string syntax = string.Format(CultureInfo.InvariantCulture,
            @"SELECT table_schema 
            FROM INFORMATION_SCHEMA.TABLES 
            WHERE TABLE_NAME = '{0}'", tableInfo.TableName);
            if (!string.IsNullOrEmpty(DatabaseName))
                syntax += string.Format(" AND TABLE_SCHEMA = '{0}'", DatabaseName);

            return syntax;
        }
    }
}