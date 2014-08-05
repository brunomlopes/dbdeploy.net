namespace Net.Sf.Dbdeploy.Database
{
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using NVelocity.Exception;

    /// <summary>
    /// The Database Management System syntax.
    /// </summary>
    public abstract class DbmsSyntax : IDbmsSyntax
    {
        /// <summary>
        /// The Regex for removing the schema name from a table.
        /// </summary>
        public static readonly Regex SchemaRegex = new Regex(@"^(?<Schema>.*?)\.", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// The change log table token to replace in the script without any schema.
        /// </summary>
        private const string ChangeLogTableToken = "$(TableName)";

        /// <summary>
        /// The change log fully qualified table name token, including schema (dbo.ChangeLog).
        /// </summary>
        private const string ChangeLogQualifiedTableNameToken = "$(QualifiedTableName)";

        /// <summary>
        /// The change log schema name token to replace.
        /// </summary>
        private const string ChangeLogSchemaNameToken = "$(SchemaName)";

        /// <summary>
        /// Database name attribute
        /// </summary>
        protected string DatabaseName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbmsSyntax" /> class.
        /// </summary>
        /// <param name="dbms">The DBMS.</param>
        protected DbmsSyntax(string dbms)
        {
            Dbms = dbms;
        }

        /// <summary>
        /// Virtual method to set default database name
        /// </summary>
        /// <param name="connectionString"></param>
        public virtual void SetDefaultDatabaseName(string connectionString)
        {
            DatabaseName = string.Empty;
        }

        /// <summary>
        /// Gets the default schema for a table.
        /// </summary>
        /// <value>Default schema.</value>
        public virtual string DefaultSchema
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Gets the syntax to get the current timestamp.
        /// </summary>
        /// <value>
        /// The current timestamp syntax.
        /// </value>
        public abstract string CurrentTimestamp { get; }

        /// <summary>
        /// Gets the syntax to get the current user.
        /// </summary>
        /// <value>
        /// The current user syntax.
        /// </value>
        public abstract string CurrentUser { get; }

        /// <summary>
        /// The DBMS type (mssql, mysql, ora).
        /// </summary>
        public string Dbms { get; private set; }

        /// <summary>
        /// Gets the table name and schema.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>Table name and schema.</returns>
        public TableInfo GetTableInfo(string tableName)
        {
            var info = new TableInfo { TableName = tableName, Schema = DefaultSchema };

            // Splite schema out if it is specified.
            var match = SchemaRegex.Match(tableName);
            if (match.Success)
            {
                info.Schema = match.Groups["Schema"].Value;
                info.TableName = SchemaRegex.Replace(tableName, string.Empty);
            }

            return info;
        }

        /// <summary>
        /// Gets the syntax for checking if a table exists.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>SQL for checking if a table exists.</returns>
        public virtual string TableExists(string tableName)
        {
            var tableInfo = GetTableInfo(tableName);
            return GetQueryTableExists(tableInfo);
        }

        protected virtual string GetQueryTableExists(TableInfo tableInfo)
        {
            var syntax = string.Format(CultureInfo.InvariantCulture,
            @"SELECT table_schema 
            FROM INFORMATION_SCHEMA.TABLES 
            WHERE TABLE_NAME = '{0}'", tableInfo.TableName.ToLower());
            if (!string.IsNullOrWhiteSpace(tableInfo.Schema))
                syntax += string.Format(" AND TABLE_SCHEMA = '{0}'", tableInfo.Schema);

            return syntax;
        }


        public string GetTemplateFileNameFor(string templateQualifier)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}_{1}.vm", Dbms, templateQualifier);
        }

        /// <summary>
        /// Gets the Change Log Table create script.
        /// </summary>
        /// <param name="tableName">Name of the change log table.</param>
        /// <returns>
        /// Script to create a Change Log table for the current DBMS.
        /// </returns>
        public string CreateChangeLogTableSqlScript(string tableName)
        {
            string script;

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = string.Format("Net.Sf.Dbdeploy.Resources.CreateSchemaVersionTable.{0}.sql", Dbms);
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new ResourceNotFoundException(string.Format("The required resource '{0}' was not found in the assembly.", resourceName));
                }

                using (var reader = new StreamReader(stream))
                {
                    script = reader.ReadToEnd();
                }
            }

            var tableInfo = GetTableInfo(tableName);

            return script
                .Replace(ChangeLogQualifiedTableNameToken, tableName)
                .Replace(ChangeLogTableToken, tableInfo.TableName)
                .Replace(ChangeLogSchemaNameToken, tableInfo.Schema);
        }
    }
}