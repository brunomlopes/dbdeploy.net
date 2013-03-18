namespace Net.Sf.Dbdeploy.Database
{
    using System.Globalization;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// The Database Management System syntax.
    /// </summary>
    public abstract class DbmsSyntax : IDbmsSyntax
	{
        private const string ChangeLogTableToken = "TABLE_NAME";

        /// <summary>
        /// The DBMS type (mssql, mysql, ora).
        /// </summary>
        private readonly string dbms;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbmsSyntax" /> class.
        /// </summary>
        /// <param name="dbms">The DBMS.</param>
        protected DbmsSyntax(string dbms)
        {
            this.dbms = dbms;
        }

        public abstract string GenerateTimestamp();

		public abstract string GenerateUser();

        /// <summary>
        /// Gets the Change Log Table create script.
        /// </summary>
        /// <param name="tableName">Name of the change log table.</param>
        /// <returns>
        /// Script to create a Change Log table for the current DBMS.
        /// </returns>
        public string CreateChangeLogTable(string tableName)
        {
            string script = string.Empty;

            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = string.Format("Net.Sf.Dbdeploy.Resources.CreateSchemaVersionTable.{0}.sql", this.dbms);
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    script = reader.ReadToEnd();
                }
            }

            return script.Replace(ChangeLogTableToken, tableName);
        }
	}
}