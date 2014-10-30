using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using Net.Sf.Dbdeploy.Database;

namespace Net.Sf.Dbdeploy.Utils
{
    public abstract class DatabaseTestBase
    {
        /// <summary>
        /// The table name for the change tracking table.
        /// </summary>
        public const string ChangeLogTableName = "ChangeLog";

        protected readonly string ConnectionString;

        protected DatabaseTestBase(string connectionString)
        {
            ConnectionString = connectionString;
        }

        /// <summary>
        /// Drops the table to ensure it does not exist.
        /// </summary>
        /// <param name="name">The name.</param>
        protected void EnsureTableDoesNotExist(string name)
        {
            this.ExecuteSql(string.Format(CultureInfo.InvariantCulture,
                @"IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND TYPE IN (N'U'))
                DROP TABLE [dbo].[{0}]", name));
        }

        protected void VerificarSeGravouAMensagemDeErroEStatusCorreto()
        {
            const string selectStatus = "SELECT ScriptStatus FROM ChangeLog";
            const string selectOutput = "SELECT ScriptOutput FROM ChangeLog";

            var status = ExecuteScalar<byte>(selectStatus);
            var output = ExecuteScalar<string>(selectOutput);

            status.Should().Be(0, "Status do script deve ser 0 - Erro");
            output.Should().NotBeNullOrEmpty("Deve conter uma mensagem de erro");
        }


        /// <summary>
        /// Asserts the table does exist.
        /// </summary>
        /// <param name="name">The name of the table.</param>
        protected void AssertTableExists(string name)
        {
            var syntax = new MsSqlDbmsSyntax();
            var schema = this.ExecuteScalar<int>(syntax.TableExists(name));

            schema.Should().BeGreaterThan(0);
        }

        /// <summary>
        /// Executes a query returning a scalar value.
        /// </summary>
        /// <typeparam name="T">Scalar value to be returned.</typeparam>
        /// <param name="sql">The SQL.</param>
        /// <param name="args">The arguments to format into the script.</param>
        /// <returns>
        /// Scalar value from query.
        /// </returns>
        protected T ExecuteScalar<T>(string sql, params object[] args)
        {
            T result;
            using (var connection = this.GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = string.Format(CultureInfo.InvariantCulture, sql, args);
                result = (T)command.ExecuteScalar();
            }

            return result;
        }

        /// <summary>
        /// Executes the SQL statement.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        private void ExecuteSql(string sql)
        {
            using (var connection = this.GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = sql;
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Gets a database connection.
        /// </summary>
        /// <returns>Database connection.</returns>
        private IDbConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }
    }
}