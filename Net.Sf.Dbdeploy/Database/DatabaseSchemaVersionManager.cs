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

        public DatabaseSchemaVersionManager(DbmsFactory factory, string deltaSet)
        {
            this.factory = factory;
            this.deltaSet = deltaSet;
        }

        public DbmsSyntax DbmsSyntax
        {
            get { return factory.CreateDbmsSyntax(); }
        }

        public DbConnection Connection
        {
            get { return factory.CreateConnection(); }
        }

        public List<int> GetAppliedChangeNumbers()
        {
            List<int> changeNumbers = new List<int>();

            try
            {
                using (DbConnection connection = Connection)
                {
                    connection.Open();

                    DbCommand command = connection.CreateCommand();

                    command.CommandText = "SELECT change_number FROM dbo." + TABLE_NAME +
                                          " WHERE delta_set = @delta_set ORDER BY change_number";
                    DbParameter parameter = command.CreateParameter();
                    parameter.ParameterName = "@delta_set";
                    parameter.Value = deltaSet;
                    command.Parameters.Add(parameter);

                    using (DbDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            changeNumbers.Add(reader.GetInt32(0));
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

            builder.Append("--------------- Fragment begins: " + changeScript + " ---------------\n");
            builder.Append("INSERT INTO " + TABLE_NAME +
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

            builder.Append("UPDATE " + TABLE_NAME + " SET complete_dt = "
                           + DbmsSyntax.GenerateTimestamp()
                           + " WHERE change_number = " + changeScript.GetId()
                           + " AND delta_set = '" + deltaSet + "'"
                           + DbmsSyntax.GenerateStatementDelimiter());
            builder.Append(DbmsSyntax.GenerateCommit());
            builder.Append("\n--------------- Fragment ends: " + changeScript + " ---------------\n");
            return builder.ToString();
        }

        public string GenerateUndoDeltaFragmentFooter(ChangeScript changeScript)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("DELETE FROM " + TABLE_NAME
                           + " WHERE change_number = " + changeScript.GetId()
                           + " AND delta_set = '" + deltaSet + "'"
                           + DbmsSyntax.GenerateStatementDelimiter());
            builder.Append(DbmsSyntax.GenerateCommit());
            builder.Append("\n--------------- Fragment ends: " + changeScript + " ---------------\n");
            return builder.ToString();
        }
    }
}