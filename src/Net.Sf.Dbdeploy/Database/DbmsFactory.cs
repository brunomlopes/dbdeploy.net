using Net.Sf.Dbdeploy.Configuration;
using Net.Sf.Dbdeploy.Exceptions;

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
            provider = new DbProviderFile().LoadProviders().GetProvider(dbms);
        }

        public virtual IDbmsSyntax CreateDbmsSyntax()
        {
            switch (dbms)
            {
                case BancosSuportados.ORACLE:
                    return new OracleDbmsSyntax();
                case BancosSuportados.MSSQL:
                    return new MsSqlDbmsSyntax();
                case BancosSuportados.MYSQL:
                    return new MySqlDbmsSyntax();
                case BancosSuportados.FIREBIRD:
                    return new FirebirdDbmsSyntax();
                case BancosSuportados.POSTGRE:
                    return new PostgreDbmsSyntax();
                case BancosSuportados.SYBASE:
                    return new SybaseDbmsSyntax();
                default:
                    throw new DbmsNotSupportedException("Supported dbms: ora, mssql, mysql, firebird, postgre, sybase");
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