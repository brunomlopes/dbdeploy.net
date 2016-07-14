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
            IDbmsSyntax dmDbmsSyntax;
            
            switch (dbms)
            {
                case SupportedDbms.ORACLE:
                    dmDbmsSyntax = new OracleDbmsSyntax();
                    break;
                case SupportedDbms.ORACLE8i:
                    dmDbmsSyntax = new Oracle8iDbmsSyntax();
                    break;
                case SupportedDbms.MSSQL:
                    dmDbmsSyntax = new MsSqlDbmsSyntax();
                    break;
                case SupportedDbms.MYSQL:
                    dmDbmsSyntax = new MySqlDbmsSyntax();
                    break;
                case SupportedDbms.FIREBIRD:
                    dmDbmsSyntax = new FirebirdDbmsSyntax();
                    break;
                case SupportedDbms.POSTGRE:
                    dmDbmsSyntax = new PostgreDbmsSyntax();
                    break;
                case SupportedDbms.SYBASE:
                    dmDbmsSyntax = new SybaseDbmsSyntax();
                    break;
                default:
                    throw new DbmsNotSupportedException("Supported dbms: ora, mssql, mysql, firebird, postgre, sybase");
            }
            dmDbmsSyntax.SetDefaultDatabaseName(connectionString);
            return dmDbmsSyntax;
        }

        public virtual IDbConnection CreateConnection()
        {
            var assembly = dllPathConnection != null ? Assembly.LoadFrom(dllPathConnection) : Assembly.Load(provider.AssemblyName);
            var type = assembly.GetType(provider.ConnectionClass);
            return (IDbConnection)Activator.CreateInstance(type, connectionString);
        }
    }
}