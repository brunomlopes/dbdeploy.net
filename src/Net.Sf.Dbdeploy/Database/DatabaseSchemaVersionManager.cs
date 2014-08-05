namespace Net.Sf.Dbdeploy.Database
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Globalization;

    using Exceptions;
    using Scripts;

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
        public DatabaseSchemaVersionManager(QueryExecuter queryExecuter,
                                            IDbmsSyntax syntax,
                                            string changeLogTableName)
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
            if (!ChangeLogTableExists())
            {
                return new List<ChangeEntry>();
            }

            var changes = new List<ChangeEntry>();
            try
            {
                // Find all changes that are not resolved.
                var sql = string.Format(CultureInfo.InvariantCulture, "SELECT ChangeId, Folder, ScriptNumber, ScriptName, ScriptStatus, ScriptOutput FROM {0}", changeLogTableName);

                using (var reader = queryExecuter.ExecuteQuery(sql))
                {
                    while (reader.Read())
                    {
                        var folder = GetValue<string>(reader, "Folder");
                        var scriptNumber = GetShort(reader, "ScriptNumber");
                        var changeEntry = new ChangeEntry(folder, scriptNumber);
                        changeEntry.ChangeId = GetValue<string>(reader, "ChangeId");
                        changeEntry.ScriptName = GetValue<string>(reader, "ScriptName");
                        changeEntry.Status = (ScriptStatus)GetByte(reader, "ScriptStatus");
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
            using (var reader = queryExecuter.ExecuteQuery(syntax.TableExists(changeLogTableName)))
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
            return string.Format(CultureInfo.InvariantCulture, "DELETE FROM {0} WHERE Folder = '{1}' AND ScriptNumber = {2}", changeLogTableName, script.Folder, script.ScriptNumber);
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
                output = string.IsNullOrEmpty(output) ? " " : RemoveInvalidCharacters(output);
                var completeDateValue = status != ScriptStatus.Started ? syntax.CurrentTimestamp : "NULL";

                if (string.IsNullOrWhiteSpace(script.ChangeId))
                {
                    var sqlInsert = string.Format(
                    CultureInfo.InvariantCulture,
                    @"INSERT INTO {0} (ChangeId, Folder, ScriptNumber, ScriptName, StartDate, CompleteDate, AppliedBy, ScriptStatus, ScriptOutput) VALUES ('{1}', '{2}', {3}, '{4}', {5}, {6}, {7}, {8}, '{9}')",
                    changeLogTableName,
                    Guid.NewGuid(),
                    script.Folder,
                    script.ScriptNumber,
                    script.ScriptName,
                    syntax.CurrentTimestamp,
                    completeDateValue,
                    syntax.CurrentUser,
                    (int)status,
                    output);

                    queryExecuter.Execute(sqlInsert);

                    var sqlSelect = string.Format("SELECT ChangeId FROM {0} WHERE Folder = '{1}' and ScriptNumber = {2}", changeLogTableName, script.Folder, script.ScriptNumber);

                    using (var reader = queryExecuter.ExecuteQuery(sqlSelect))
                    {
                        reader.Read();
                        script.ChangeId = (reader.GetString(0));
                    }
                }
                else
                {
                    var sql = string.Format(
                        CultureInfo.InvariantCulture,
                        "UPDATE {0} SET Folder = '{1}', ScriptNumber = {2}, ScriptName = '{3}', {4}CompleteDate = {5}, AppliedBy = {6}, ScriptStatus = {7}, ScriptOutput = '{8}' WHERE ChangeId = '{9}'",
                        changeLogTableName,
                        script.Folder,
                        script.ScriptNumber,
                        script.ScriptName,
                        status == ScriptStatus.Started ? string.Format(CultureInfo.InvariantCulture, "StartDate = {0}, ", syntax.CurrentTimestamp) : string.Empty,
                        completeDateValue,
                        syntax.CurrentUser,
                        (int)status,
                        output,
                        script.ChangeId);

                    queryExecuter.Execute(sql);
                }
            }
            catch (DbException e)
            {
                throw new SchemaVersionTrackingException("Could not update change log because: " + e.Message, e);
            }
        }

        private string RemoveInvalidCharacters(string output)
        {
            return output.Replace("'", string.Empty);
        }

        /// <summary>
        /// Gets the value from the <see cref="IDataReader"/> to the specified type if it is not DBNull.
        /// </summary>
        /// <typeparam name="T">Type of value to retrieve.</typeparam>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the column.</param>
        /// <returns>Value if not null; otherwise default.</returns>
        private T GetValue<T>(IDataReader reader, string name)
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

        private short GetShort(IDataReader reader, string name)
        {
            // Handle DBNull values.
            var columnValue = reader[name];
            if (columnValue != DBNull.Value)
            {
                return Convert.ToInt16(columnValue);
            }
            return 0;
        }

        private short GetByte(IDataReader reader, string name)
        {
            // Handle DBNull values.
            var columnValue = reader[name];
            if (columnValue != DBNull.Value)
            {
                return Convert.ToByte(columnValue);
            }
            return 0;
        }

        /// <summary>
        /// Creates the change log table in the database.
        /// </summary>
        public void CreateChangeLogTable()
        {
            // Get table creation script from embeded file.
            var script = syntax.CreateChangeLogTableSqlScript(changeLogTableName);
            queryExecuter.Execute(script);
        }
    }
}
