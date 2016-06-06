namespace Net.Sf.Dbdeploy.Database
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Globalization;

    using Net.Sf.Dbdeploy.Exceptions;
    using Net.Sf.Dbdeploy.Scripts;

    /// <summary>
    /// Manages updating the change log table in the database, and retrieving applied changes.
    /// </summary>
    public class DatabaseSchemaVersionManager : IAppliedChangesProvider
    {
        /// <summary>
        /// The query executer for getting and updating the change log table.
        /// </summary>
        private readonly QueryExecuter queryExecuter;

        /// <summary>
        /// The change log table name.
        /// </summary>
        private readonly string changeLogTableName;

        /// <summary>
        /// The syntax for the current Database Management System.
        /// </summary>
        private readonly IDbmsSyntax syntax;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseSchemaVersionManager" /> class.
        /// </summary>
        /// <param name="queryExecuter">The query executer.</param>
        /// <param name="syntax">The syntax.</param>
        /// <param name="changeLogTableName">Name of the change log table.</param>
        /// <param name="autoCreateChangeLogTable">if set to <c>true</c> the change log table will automatically be created.</param>
        public DatabaseSchemaVersionManager(QueryExecuter queryExecuter, IDbmsSyntax syntax, string changeLogTableName)
        {
            this.syntax = syntax;
            this.queryExecuter = queryExecuter;
            this.changeLogTableName = changeLogTableName;
        }

        /// <summary>
        /// Gets the applied changes from the database..
        /// </summary>
        /// <returns>List of applied changes.</returns>
        /// <exception cref="SchemaVersionTrackingException">Could not retrieve change log from database because:  + e.Message</exception>
        public virtual IList<ChangeEntry> GetAppliedChanges()
        {
            if (!this.ChangeLogTableExists())
            {
                return new List<ChangeEntry>();
            }

            var changes = new List<ChangeEntry>();
            try
            {
                // Find all changes that are not resolved.
                string sql = string.Format(CultureInfo.InvariantCulture, "SELECT ChangeId, Folder, ScriptNumber, ScriptName, ScriptStatus, ScriptOutput FROM {0}", this.changeLogTableName);
                
                using (var reader = this.queryExecuter.ExecuteQuery(sql))
                {
                    while (reader.Read())
                    {
                        var folder = GetValue<string>(reader, "Folder");
                        var scriptNumber = GetValue<short>(reader, "ScriptNumber");
                        var changeEntry = new ChangeEntry(folder, scriptNumber);
                        changeEntry.ChangeId = GetValue<int>(reader, "ChangeId");
                        changeEntry.ScriptName = GetValue<string>(reader, "ScriptName");
                        // SQL Server, PGSQL use byte, but mysql uses sbyte, 
                        // so use Convert instead of cast to work with both cases
                        changeEntry.Status = (ScriptStatus)Convert.ToByte(GetValue<object>(reader, "ScriptStatus"));
                        changeEntry.Output = GetValue<string>(reader, "ScriptOutput");

                        changes.Add(changeEntry);
                    }
                }

                // Make sure everything is in correct ordering.
                changes.Sort();

                return changes;
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
        /// <exception cref="ChangelogTableDoesNotExistException">Thrown when the change log table is not found.</exception>
        public virtual bool ChangeLogTableExists()
        {
            using (var reader = this.queryExecuter.ExecuteQuery(this.syntax.TableExists(this.changeLogTableName)))
            {
                return reader.Read();
            }
        }

        /// <summary>
        /// Gets the SQL to delete a change log entry.
        /// </summary>
        /// <param name="script">The script.</param>
        /// <returns>SQL to delete the change log entry.</returns>
        public virtual string GetChangelogDeleteSql(ChangeScript script)
        {
            return string.Format(CultureInfo.InvariantCulture, "DELETE FROM {0} WHERE Folder = '{1}' AND ScriptNumber = {2}", this.changeLogTableName, script.Folder, script.ScriptNumber);
        }

        /// <summary>
        /// Records the script status.
        /// </summary>
        /// <param name="script">The script.</param>
        /// <param name="status">The status.</param>
        /// <param name="output">The output of the script.</param>
        /// <exception cref="SchemaVersionTrackingException">Could not update change log because:  + e.Message</exception>
        public virtual void RecordScriptStatus(ChangeScript script, ScriptStatus status, string output = null)
        {
            try
            {
                // Insert or update based on if there is a change ID.
                // Update complete date for all but started.
                var completeDateValue = status != ScriptStatus.Started ? this.syntax.CurrentTimestamp : "NULL";
                if (script.ChangeId == 0)
                {
					var insertSql = string.Format(
						CultureInfo.InvariantCulture,
"INSERT INTO {0} (Folder, ScriptNumber, ScriptName, StartDate, CompleteDate, AppliedBy, ScriptStatus, ScriptOutput) VALUES (@1, @2, @3, {1}, {2}, {3}, @4, @5)",
						this.changeLogTableName,
						this.syntax.CurrentTimestamp,
						completeDateValue,
						this.syntax.CurrentUser);

					// Execute insert.
					this.queryExecuter.ExecuteQuery(insertSql, script.Folder, script.ScriptNumber, script.ScriptName, (int)status, output ?? string.Empty);


                    var sql = string.Format(
                        CultureInfo.InvariantCulture,
"SELECT ChangeId FROM {0} WHERE Folder = @1 and ScriptNumber = @2",
                        this.changeLogTableName);

                    // Execute set change id so it can be updated.
                    using (var reader = this.queryExecuter.ExecuteQuery(sql, script.Folder, script.ScriptNumber))
                    {
                        reader.Read();
                        script.ChangeId = reader.GetInt32(0);
                    }
                }
                else
                {
                    // Update existing entry.
                    var sql = string.Format(
                        CultureInfo.InvariantCulture,
                        "UPDATE {0} SET Folder = @1, ScriptNumber = @2, ScriptName = @3, {1}CompleteDate = {2}, AppliedBy = {3}, ScriptStatus = @4, ScriptOutput = @5 WHERE ChangeId = @6",
                        this.changeLogTableName,
                        status == ScriptStatus.Started ? string.Format(CultureInfo.InvariantCulture, "StartDate = {0}, ", this.syntax.CurrentTimestamp) : string.Empty,
                        completeDateValue,
                        this.syntax.CurrentUser);

                    this.queryExecuter.Execute(sql, script.Folder, script.ScriptNumber, script.ScriptName, (int)status, output ?? string.Empty, script.ChangeId);
                }
            }
            catch (DbException e)
            {
                throw new SchemaVersionTrackingException("Could not update change log because: " + e.Message, e);
            }
        }

        /// <summary>
        /// Gets the value from the <see cref="IDataReader"/> to the specified type if it is not DBNull.
        /// </summary>
        /// <typeparam name="T">Type of value to retrieve.</typeparam>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the column.</param>
        /// <returns>Value if not null; otherwise default.</returns>
        private static T GetValue<T>(IDataReader reader, string name)
        {
            var value = default(T);

            // Handle DBNull values.
            var columnValue = reader[name];
            if (columnValue != DBNull.Value)
            {
                value = (T)columnValue;
            }

            return value;
        }

        /// <summary>
        /// Creates the change log table in the database.
        /// </summary>
        public void CreateChangeLogTable()
        {
            // Get table creation script from embeded file.
            string script = this.syntax.CreateChangeLogTableSqlScript(this.changeLogTableName);
            this.queryExecuter.Execute(script);
        }
    }
}
