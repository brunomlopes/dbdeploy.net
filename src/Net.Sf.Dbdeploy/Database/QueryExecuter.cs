using System;
using System.Data;

namespace Net.Sf.Dbdeploy.Database
{
    public class QueryExecuter
    {
        private readonly IDbConnection connection;

        private IDbTransaction transaction;

        public QueryExecuter(DbmsFactory factory)
        {
            this.connection = factory.CreateConnection();

            this.connection.Open();
        }

        public IDataReader ExecuteQuery(string sql)
        {
            using (IDbCommand command = this.connection.CreateCommand())
            {
                command.CommandText = sql;

                return command.ExecuteReader();
            }
        }

        public IDataReader Execute(string sql, params object[] parameters)
        {
            using (IDbCommand command = this.connection.CreateCommand())
            {
                command.CommandText = sql;

                if (parameters != null)
                {
                    for (int i = 1; i <= parameters.Length; i++)
                    {
                        IDbDataParameter parameterObject = command.CreateParameter();

                        parameterObject.ParameterName = i.ToString();
                        parameterObject.Value = parameters[i];

                        command.Parameters.Add(parameterObject);
                    }
                }

                return command.ExecuteReader();
            }
        }

        public void Execute(string sql)
        {
            using (IDbCommand command = this.connection.CreateCommand())
            {
                command.CommandText = sql;

                command.ExecuteNonQuery();
            }
        }

        public void BeginTransaction()
        {
            if (this.transaction != null)
                throw new InvalidOperationException("There is already an open transaction.");

            this.transaction = this.connection.BeginTransaction();
        }

        public void CommitTransaction()
        {
            if (this.transaction == null)
                throw new InvalidOperationException("There is no open transaction.");

            this.transaction.Commit();

            this.transaction.Dispose();
            this.transaction = null;
        }

        public void Close()
        {
            if (this.connection.State == ConnectionState.Open)
            {
                this.connection.Close();
            }
        }
    }
}
