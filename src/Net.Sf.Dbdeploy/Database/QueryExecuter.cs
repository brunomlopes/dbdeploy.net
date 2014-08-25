namespace Net.Sf.Dbdeploy.Database
{
    using System;
    using System.Data;
    using System.Text;

    public class QueryExecuter
    {
        private readonly IDbConnection connection;

        private IDbTransaction transaction;

        /// <summary>
        /// The output of the current execution.
        /// </summary>
        private StringBuilder currentOutput;

        public QueryExecuter(DbmsFactory factory)
        {
            connection = factory.CreateConnection();

            connection.Open();

            currentOutput = null;

            AttachInfoMessageEventHandler(connection);
        }

        /// <summary>
        /// Executes the query.
        /// </summary>
        /// <param name="sql">The SQL to run.</param>
        /// <param name="parameters">The parameters to format into the SQL.</param>
        /// <returns>
        /// Data reader to get records.
        /// </returns>
        public virtual IDataReader ExecuteQuery(string sql, params object[] parameters)
        {
            return ExecuteQuery(sql, null, parameters);
        }

        /// <summary>
        /// Executes the query.
        /// </summary>
        /// <param name="sql">The SQL to run.</param>
        /// <param name="output">The output of the SQL execution.</param>
        /// <param name="parameters">The parameters to format into the SQL.</param>
        /// <returns>
        /// Data reader to get records.
        /// </returns>
        public virtual IDataReader ExecuteQuery(string sql, StringBuilder output, params object[] parameters)
        {
            // Capture output to string builder specified.
            currentOutput = output;

            var command = CreateCommand();
            command.CommandText = sql;

            if (parameters != null)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    var parameterObject = command.CreateParameter();

                    parameterObject.ParameterName = (i + 1).ToString();
                    parameterObject.Value = parameters[i];

                    command.Parameters.Add(parameterObject);
                }
            }
            return command.ExecuteReader();
        }

        /// <summary>
        /// Executes the specified SQL.
        /// </summary>
        /// <param name="sql">The SQL to run.</param>
        /// <param name="parameters">The parameters to format into the SQL.</param>
        public virtual void Execute(string sql, params object[] parameters)
        {
            Execute(sql, null, parameters);
        }

        /// <summary>
        /// Executes the specified SQL.
        /// </summary>
        /// <param name="sql">The SQL to run.</param>
        /// <param name="output">The output from the running of the script..</param>
        /// <param name="parameters">The parameters to format into the SQL.</param>
        public virtual void Execute(string sql, StringBuilder output, params object[] parameters)
        {
            // Capture output to string builder specified.
            currentOutput = output;

            using (var command = CreateCommand())
            {
                command.CommandText = sql;

                if (parameters != null)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var parameterObject = command.CreateParameter();

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
            using (var command = CreateCommand())
            {
                command.CommandText = sql;
                command.ExecuteNonQuery();
            }
        }

        public virtual void BeginTransaction()
        {
            if (transaction != null)
                throw new InvalidOperationException("There is already an open transaction.");

            transaction = connection.BeginTransaction();
        }

        public virtual void CommitTransaction()
        {
            if (transaction == null)
                throw new InvalidOperationException("There is no open transaction.");

            transaction.Commit();

            transaction.Dispose();
            transaction = null;
        }

        public virtual void Close()
        {
            if (connection.State == ConnectionState.Open)
            {
                connection.Close();
            }
        }

        /// <summary>
        /// Triggered when a message is received from the query execution.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The args.</param>
        public void InfoMessageEventHandler(object sender, EventArgs args)
        {
            var propertyInfo = args.GetType().GetProperty("Message");

            if (currentOutput != null && propertyInfo != null)
            {
                currentOutput.AppendLine(((dynamic)args).Message);
            }
        }

        /// <summary>
        /// Attaches the info message event handler to the connection if possible.
        /// </summary>
        /// <param name="dbConnection">The database connection.</param>
        private void AttachInfoMessageEventHandler(IDbConnection dbConnection)
        {
            try
            {
                // The connections support an InfoMessage event, but do not expose it at a common class or interface level.
                // Attach to the event here to capture output.
                var eventInfo = dbConnection.GetType().GetEvent("InfoMessage");
                if (eventInfo != null)
                {
                    var methodInfo = GetType().GetMethod("InfoMessageEventHandler", new[] { typeof(object), typeof(EventArgs) });
                    var handler = Delegate.CreateDelegate(eventInfo.EventHandlerType, this, methodInfo, true);
                    eventInfo.AddEventHandler(dbConnection, handler);
                }
            }
            catch
            {
                // Suppress any errors attempting to attach.
            }
        }

        private IDbCommand CreateCommand()
        {
            var command = connection.CreateCommand();

            if (transaction != null)
            {
                command.Transaction = transaction;
            }

            return command;
        }
    }
}
