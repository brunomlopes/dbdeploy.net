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

        public virtual IDataReader ExecuteQuery(string sql, params object[] parameters)
        {
            using (IDbCommand command = this.CreateCommand())
            {
                command.CommandText = sql;

                if (parameters != null)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        IDbDataParameter parameterObject = command.CreateParameter();

                        parameterObject.ParameterName = (i + 1).ToString();
                        parameterObject.Value = parameters[i];

                        command.Parameters.Add(parameterObject);
                    }
                }

                return command.ExecuteReader();
            }
        }

        public virtual void Execute(string sql, params object[] parameters)
        {
            using (IDbCommand command = this.CreateCommand())
            {
                command.CommandText = sql;

                if (parameters != null)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        IDbDataParameter parameterObject = command.CreateParameter();

                        parameterObject.ParameterName = (i + 1).ToString();
                        parameterObject.Value = parameters[i];

                        command.Parameters.Add(parameterObject);
                    }
                }

                command.ExecuteNonQuery();
            }
        }

        public virtual void Execute(string sql)
        {
            using (IDbCommand command = this.CreateCommand())
            {
                command.CommandText = sql;

                command.ExecuteNonQuery();
            }
        }

        public virtual void BeginTransaction()
        {
            if (this.transaction != null)
                throw new InvalidOperationException("There is already an open transaction.");

            this.transaction = this.connection.BeginTransaction();
        }

        public virtual void CommitTransaction()
        {
            if (this.transaction == null)
                throw new InvalidOperationException("There is no open transaction.");

            this.transaction.Commit();

            this.transaction.Dispose();
            this.transaction = null;
        }

        public virtual void Close()
        {
            if (this.connection.State == ConnectionState.Open)
            {
                this.connection.Close();
            }
        }

        private IDbCommand CreateCommand()
        {
            IDbCommand command = this.connection.CreateCommand();

            if (this.transaction != null)
            {
                command.Transaction = this.transaction;
            }

            return command;
        }
    }
}
