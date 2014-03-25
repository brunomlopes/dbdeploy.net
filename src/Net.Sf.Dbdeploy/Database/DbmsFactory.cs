using System.IO;

namespace Net.Sf.Dbdeploy.Database
{
    using System;
    using System.Data;
    using System.Reflection;

    public class DbmsFactory
    {
        private readonly string dbms;
        private readonly string connectionString;
        private readonly DatabaseProvider provider;
        private readonly string dllPathConnection;

        public DbmsFactory(string dbms, string connectionString, string dllPathConnection = null)
        {
            this.dbms = dbms;
            this.connectionString = connectionString;
            this.dllPathConnection = dllPathConnection;
            this.provider = new DbProviderFile().LoadProviders().GetProvider(dbms);
        }

        public virtual IDbmsSyntax CreateDbmsSyntax()
        {
            switch (this.dbms)
            {
                case "ora":
                    return new OracleDbmsSyntax();
                case "mssql":
                    return new MsSqlDbmsSyntax();
                case "mysql":
                    return new MySqlDbmsSyntax();
                case "firebird":
                    return new FirebirdDbmsSyntax();
                case "postgres":
                    return new PostgresDbmsSyntax();
                default:
                    throw new ArgumentException("Supported dbms: ora, mssql, mysql, firebird, postgres");
            }
        }

        public virtual IDbConnection CreateConnection()
        {
            var assembly = dllPathConnection != null ? Assembly.LoadFrom(dllPathConnection) : Assembly.Load(provider.AssemblyName);
            var type = assembly.GetType(provider.ConnectionClass);
            return (IDbConnection)Activator.CreateInstance(type, connectionString);
        }
    }
}