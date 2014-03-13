using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Database
{
    abstract class DbmsFactoryAbstractTest
    {
        private IDbConnection connection;

        public abstract void GivenAnEntryWithoutDllConnectionShouldThrowsException();
        public abstract void GivenAnEntryWithDllConnectionShouldOpenedSuccessful();
        public abstract void ShouldExecuteSqlCommandSuccessful();
        public abstract void ShouldExecuteSqlCommandFailed();

        protected IDbConnection OpenConnection(DbmsFactory factory)
        {
            connection = factory.CreateConnection();
            connection.Open();
            return connection;
        }

        protected void CloseConnection()
        {
            connection.Close();
        }

        protected void ExecuteNonQuery(string query)
        {
            var command = connection.CreateCommand();
            command.CommandText = query;
            command.ExecuteNonQuery();
        }
    }
}