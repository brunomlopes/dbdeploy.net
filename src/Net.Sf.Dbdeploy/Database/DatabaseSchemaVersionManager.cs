using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using Net.Sf.Dbdeploy.Exceptions;
using Net.Sf.Dbdeploy.Scripts;

namespace Net.Sf.Dbdeploy.Database
{
    public class DatabaseSchemaVersionManager : IAppliedChangesProvider
    {
        private readonly QueryExecuter queryExecuter;

        private readonly string changeLogTableName;

        private readonly IDbmsSyntax syntax;

        public DatabaseSchemaVersionManager(QueryExecuter queryExecuter, IDbmsSyntax syntax, string changeLogTableName)
        {
            this.syntax = syntax;
            this.queryExecuter = queryExecuter;
            this.changeLogTableName = changeLogTableName;
        }

    	public virtual ICollection<int> GetAppliedChanges()
    	{
    	    using (IDataReader reader = queryExecuter.ExecuteQuery(@"
SELECT table_schema 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME = @1", changeLogTableName))
            {
                if(!reader.Read())
                {
                    throw new ChangelogTableDoesNotExistException(string.Format("No table found with name '{0}'.", changeLogTableName));
                }
            }

            List<int> changeNumbers = new List<int>();
            try
            {
                string sql = string.Format(CultureInfo.InvariantCulture, "SELECT change_number FROM {0} ORDER BY change_number", this.changeLogTableName);
                
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

        public virtual string GetChangelogDeleteSql(ChangeScript script)
        {
            return string.Format(CultureInfo.InvariantCulture, "DELETE FROM {0} WHERE change_number = {1}", this.changeLogTableName, script.GetId());
        }

        public virtual void RecordScriptApplied(ChangeScript script)
        {
            try
            {
                string sql = string.Format(
                    CultureInfo.InvariantCulture,
                    "INSERT INTO {0} (change_number, complete_dt, applied_by, description) VALUES (@1, {1}, {2}, @2)", 
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
