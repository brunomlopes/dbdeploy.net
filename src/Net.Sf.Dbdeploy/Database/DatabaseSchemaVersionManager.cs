using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using Net.Sf.Dbdeploy.Exceptions;
using Net.Sf.Dbdeploy.Scripts;

namespace Net.Sf.Dbdeploy.Database
{
    using System.Reflection;

    public class DatabaseSchemaVersionManager : IAppliedChangesProvider
    {
        private readonly QueryExecuter queryExecuter;

        private readonly string changeLogTableName;

        private readonly IDbmsSyntax syntax;

        /// <summary>
        /// Gets or sets a value indicating whether the change log table should be created if it does not exist.
        /// </summary>
        /// <value>
        /// <c>true</c> if auto create change log table; otherwise, <c>false</c>.
        /// </value>
        public bool AutoCreateChangeLogTable { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseSchemaVersionManager" /> class.
        /// </summary>
        /// <param name="queryExecuter">The query executer.</param>
        /// <param name="syntax">The syntax.</param>
        /// <param name="changeLogTableName">Name of the change log table.</param>
        /// <param name="autoCreateChangeLogTable">if set to <c>true</c> the change log table will automatically be created.</param>
        public DatabaseSchemaVersionManager(QueryExecuter queryExecuter, IDbmsSyntax syntax, string changeLogTableName, bool autoCreateChangeLogTable)
        {
            this.syntax = syntax;
            this.queryExecuter = queryExecuter;
            this.changeLogTableName = changeLogTableName;
            this.AutoCreateChangeLogTable = autoCreateChangeLogTable;
        }

    	public virtual ICollection<int> GetAppliedChanges()
    	{
    	    this.VerifyChangeLogTableExists(this.AutoCreateChangeLogTable);

    	    List<int> changeNumbers = new List<int>();
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture, "SELECT ScriptNumber FROM {0} ORDER BY ScriptNumber", this.changeLogTableName);
                
                using (IDataReader reader = this.queryExecuter.ExecuteQuery(sql))
                {
                    while (reader.Read())
                    {
                        int changeNumber = Int32.Parse(reader.GetValue(0).ToString());

						changeNumbers.Add(changeNumber);
                    }
                }

                return changeNumbers;
            }
            catch (DbException e)
            {
                throw new SchemaVersionTrackingException(
                    "Could not retrieve change log from database because: " + e.Message, e);
            }            
        }

        /// <summary>
        /// Verifies the change log table exists.
        /// </summary>
        /// <param name="autoCreate">if set to <c>true</c> the table will be created if it does not exist.</param>
        /// <exception cref="ChangelogTableDoesNotExistException">Thrown when the change log table is not found.</exception>
        public void VerifyChangeLogTableExists(bool autoCreate)
        {
            bool changeLogTableExists;
            using (IDataReader reader = this.queryExecuter.ExecuteQuery(@"
SELECT table_schema 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME = @1", this.changeLogTableName))
            {
                changeLogTableExists = reader.Read();
            }

            // Create the change log table if it does not exist.
            if (!changeLogTableExists)
            {
                if (autoCreate)
                {
                    this.CreateChangeLogTable();
                }
                else
                {
                    // If change log table does not exist and is not going to be created automatically, throw an exception.
                    throw new ChangelogTableDoesNotExistException(string.Format("No table found with name '{0}'.", this.changeLogTableName));
                }
            }
        }

        /// <summary>
        /// Creates the change log table in the database.
        /// </summary>
        private void CreateChangeLogTable()
        {
            // Get table creation script from embeded file.
            string script = this.syntax.CreateChangeLogTable(this.changeLogTableName);
            this.queryExecuter.Execute(script);
        }

        public virtual string GetChangelogDeleteSql(ChangeScript script)
        {
            return string.Format(CultureInfo.InvariantCulture, "DELETE FROM {0} WHERE ScriptNumber = {1}", this.changeLogTableName, script.GetId());
        }

        public virtual void RecordScriptApplied(ChangeScript script)
        {
            try
            {
                string sql = string.Format(
                    CultureInfo.InvariantCulture,
                    "INSERT INTO {0} (Folder, ScriptNumber, FileName, StartDate, CompleteDate, AppliedBy, Status) VALUES ('Scripts', @1, @2, {1}, {1}, {2}, 1)", 
                    this.changeLogTableName,
                    this.syntax.GenerateTimestamp(),
                    this.syntax.GenerateUser());

                this.queryExecuter.Execute(
                        sql,
                        script.GetId(),
                        script.GetDescription());
            }
            catch (DbException e)
            {
                throw new SchemaVersionTrackingException("Could not update change log because: " + e.Message, e);
            }
        }
    }
}
