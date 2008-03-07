using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using Net.Sf.Dbdeploy.Exceptions;
using Net.Sf.Dbdeploy.Scripts;

namespace Net.Sf.Dbdeploy.Database
{
    public class DatabaseSchemaVersionManager
    {
        public static readonly string TABLE_NAME = "changelog";

        private readonly DbmsFactory factory;
        private readonly string deltaSet;
        private readonly int? currentVersion;

        public DatabaseSchemaVersionManager(DbmsFactory factory, string deltaSet, int? currentVersion)
        {
            this.factory = factory;
            this.deltaSet = deltaSet;
            this.currentVersion = currentVersion;
        }

        private DbmsSyntax DbmsSyntax
        {
            get { return factory.CreateDbmsSyntax(); }
        }

        private DbConnection Connection
        {
            get { return factory.CreateConnection(); }
        }

        public List<int> GetAppliedChangeNumbers()
        {
            if (currentVersion == null)
            {
                return GetCurrentVersionFromDb();
            }
            else
            {
                List<int> changeNumbers = new List<int>();
                for (int i = 1; i <= currentVersion.Value; i++)
                {
                    changeNumbers.Add(i);
                }
                return changeNumbers;
            }
        }

        private List<int> GetCurrentVersionFromDb()
        {
            List<int> changeNumbers = new List<int>();
            try
            {
                using (DbConnection connection = Connection)
                {
                    connection.Open();

                    DbCommand command = connection.CreateCommand();

                    command.CommandText = "SELECT change_number, complete_dt FROM dbo." + TABLE_NAME +
                                          " WHERE delta_set = @delta_set ORDER BY change_number";
                    DbParameter parameter = command.CreateParameter();
                    parameter.ParameterName = "@delta_set";
                    parameter.Value = deltaSet;
                    command.Parameters.Add(parameter);

                    using (DbDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                        	int changeNumber = reader.GetInt32(0);

							if (reader.IsDBNull(1))
							{
								string errorMessage = string.Format("Incompleted delta script {0} found from last execution.", changeNumber);
								throw new DbDeployException(errorMessage);
							}

							changeNumbers.Add(changeNumber);
                        }
                    }

                    return changeNumbers;
                }
            }
            catch (SqlException e)
            {
                throw new SchemaVersionTrackingException("Could not retrieve change log from database because: "
                                                         + e.Message, e);
            }            
        }

        public string GenerateDoDeltaFragmentHeader(ChangeScript changeScript)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("--------------- Fragment begins: " + changeScript + " ---------------");

            builder.AppendLine("INSERT INTO " + TABLE_NAME +
                           " (change_number, delta_set, start_dt, applied_by, description)" +
                           " VALUES (" + changeScript.GetId() + ", '" + deltaSet + "', " +
                           DbmsSyntax.GenerateTimestamp() +
                           ", " + DbmsSyntax.GenerateUser() + ", '" + changeScript.GetDescription() + "')" +
                           DbmsSyntax.GenerateStatementDelimiter());
            builder.Append(DbmsSyntax.GenerateCommit());
            return builder.ToString();
        }

        public string GenerateDoDeltaFragmentFooter(ChangeScript changeScript)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("UPDATE " + TABLE_NAME + " SET complete_dt = "
                           + DbmsSyntax.GenerateTimestamp()
                           + " WHERE change_number = " + changeScript.GetId()
                           + " AND delta_set = '" + deltaSet + "'"
                           + DbmsSyntax.GenerateStatementDelimiter());
            builder.AppendLine(DbmsSyntax.GenerateCommit());
            builder.Append("--------------- Fragment ends: " + changeScript + " ---------------");
            return builder.ToString();
        }

        public string GenerateUndoDeltaFragmentFooter(ChangeScript changeScript)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("DELETE FROM " + TABLE_NAME
                           + " WHERE change_number = " + changeScript.GetId()
                           + " AND delta_set = '" + deltaSet + "'"
                           + DbmsSyntax.GenerateStatementDelimiter());
            builder.AppendLine(DbmsSyntax.GenerateCommit());
            builder.Append("--------------- Fragment ends: " + changeScript + " ---------------");
            return builder.ToString();
        }

        public string GenerateVersionCheck()
        {
            if (currentVersion == null) return string.Empty;

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("DECLARE @currentDatabaseVersion INTEGER, @errMsg VARCHAR(1000)");
            builder.Append("SELECT @currentDatabaseVersion = MAX(change_number) FROM ").Append(TABLE_NAME).AppendLine(" WHERE delta_set = '" + deltaSet + "'");
            builder.Append("IF (@currentDatabaseVersion <> ").Append(currentVersion).AppendLine(")");
            builder.AppendLine("BEGIN");
            builder.Append("    SET @errMsg = 'Error: current database version on delta_set <").Append(deltaSet).Append("> is not ").Append(currentVersion).AppendLine(", but ' + CONVERT(VARCHAR, @currentDatabaseVersion)"); 
            builder.AppendLine("    RAISERROR (@errMsg, 16, 1)"); 
            builder.AppendLine("END");
            builder.AppendLine(DbmsSyntax.GenerateStatementDelimiter()).AppendLine();
            return builder.ToString();
        }
    }
}
