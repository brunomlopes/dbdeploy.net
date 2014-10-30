using Net.Sf.Dbdeploy.Configuration;

namespace Net.Sf.Dbdeploy.Database
{
    /// <summary>
    /// Syntax for Microsoft SQL Sever.
    /// </summary>
    public class MsSqlDbmsSyntax : DbmsSyntax
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MsSqlDbmsSyntax" /> class.
        /// </summary>
        public MsSqlDbmsSyntax() : base(SupportedDbms.MSSQL)
        {
        }

        /// <summary>
        /// Gets the syntax to get the current timestamp.
        /// </summary>
        /// <value>
        /// The current timestamp syntax.
        /// </value>
        public override string CurrentTimestamp
        {
            get { return "getdate()"; }
        }

        /// <summary>
        /// Gets the syntax to get the current user.
        /// </summary>
        /// <value>
        /// The current user syntax.
        /// </value>
        public override string CurrentUser
        {
            get { return "user_name()"; }
        }

        ///// <summary>
        ///// Gets the default schema for a table.
        ///// </summary>
        ///// <value>
        ///// Default schema.
        ///// </value>
        //public override string DefaultSchema
        //{
        //    get { return "dbo"; }
        //}

        protected override string GetQueryTableExists(TableInfo tableInfo)
        {
            var syntax = string.Format("select object_id(concat(schema_name(), '.', '{0}'))", tableInfo.TableName);
            return syntax;
        }
    }
}