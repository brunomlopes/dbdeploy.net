namespace Net.Sf.Dbdeploy.Database
{
    using System;
    using System.Data;
    using System.Reflection;

    public class DbmsFactory
    {
        private readonly string dbms;
        private readonly string connectionString;
        private readonly DbProviders providers;
        private readonly string oracleDllPath;

        public DbmsFactory(string dbms, string connectionString, string oracleDllPath = null)
        {
            this.dbms = dbms;
            this.connectionString = connectionString;
            this.oracleDllPath = oracleDllPath;
            this.providers = new DbProviderFile().LoadProviders();
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
                default:
                    throw new ArgumentException("Supported dbms: ora, mssql, mysql");
            }
        }

        public virtual IDbConnection CreateConnection()
        {

            string assemblyFullNameOracleDllPath = null;
            if (oracleDllPath != null)
            {
                assemblyFullNameOracleDllPath = AssemblyName.GetAssemblyName(oracleDllPath).FullName;
            }

            var provider = providers.GetProvider(dbms);
            var assembly = Assembly.Load(assemblyFullNameOracleDllPath ?? provider.AssemblyName);
            var type = assembly.GetType(assemblyFullNameOracleDllPath != null ? "Oracle.DataAccess.Client.OracleConnection" : provider.ConnectionClass);
            return (IDbConnection)Activator.CreateInstance(type, connectionString);
        }
    }
}