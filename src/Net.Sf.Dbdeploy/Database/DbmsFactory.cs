using System;
using System.Data.Common;
using System.Reflection;

namespace Net.Sf.Dbdeploy.Database
{
    public class DbmsFactory
    {
        private readonly string dbms;
        private readonly string connectionString;
        private readonly Providers providers;

        public DbmsFactory(string dbms, string connectionString)
        {
            this.dbms = dbms;
            this.connectionString = connectionString;
            providers = new DbProviderFile().LoadProviders();
        }

        public DbmsSyntax CreateDbmsSyntax()
        {
            switch (dbms)
            {
                case "ora":
                    return new OracleDbmsSyntax();
                case "mssql":
                    return new MsSqlDbmsSyntax();
                case "mysql":
                    return new MySqlDbmsSyntax();
                default:
                    throw new ArgumentException("Supported dbms: ora, mssql, mysql");
            }
        }

        public DbConnection CreateConnection()
        {
            DatabaseProvider provider = providers.GetProvider(dbms);
            if (provider == null) throw new ArgumentException("Supported dbms: ora, mssql, mysql.");

            Assembly assembly = Assembly.Load(provider.AssemblyName);
            Type type = assembly.GetType(provider.ConnectionClass);
            return (DbConnection) Activator.CreateInstance(type, connectionString);
        }
    }
}