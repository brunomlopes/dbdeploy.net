namespace Net.Sf.Dbdeploy.Database
{
    using System.IO;
    using System.Reflection;

    using NVelocity.Exception;

    /// <summary>
    /// The Database Management System syntax.
    /// </summary>
    public abstract class DbmsSyntax : IDbmsSyntax  
    {
        /// <summary>
        /// The change log table token to replace in the script.
        /// </summary>
        private const string ChangeLogTableToken = "TABLE_NAME";

        /// <summary>
        /// Initializes a new instance of the <see cref="DbmsSyntax" /> class.
        /// </summary>
        /// <param name="dbms">The DBMS.</param>
        protected DbmsSyntax(string dbms)
        {
            this.Dbms = dbms;
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
        /// Gets the Change Log Table create script.
        /// </summary>
        /// <param name="tableName">Name of the change log table.</param>
        /// <returns>
        /// Script to create a Change Log table for the current DBMS.
        /// </returns>
        public string CreateChangeLogTable(string tableName)
        {
            string script;

            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = string.Format("Net.Sf.Dbdeploy.Resources.CreateSchemaVersionTable.{0}.sql", this.Dbms);
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

            return script.Replace(ChangeLogTableToken, tableName);
        }
    }
}