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
        //private const string OracleConnectionClass = "Oracle.DataAccess.Client.OracleConnection";

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
                default:
                    throw new ArgumentException("Supported dbms: ora, mssql, mysql");
            }
        }

        public virtual IDbConnection CreateConnection()
        {
            string assemblyFullNameDllPath = null;
            if (dllPathConnection != null)
            {
                assemblyFullNameDllPath = AssemblyName.GetAssemblyName(dllPathConnection).FullName;
            }

            var assembly = Assembly.Load(assemblyFullNameDllPath ?? provider.AssemblyName);
            var type = assembly.GetType(provider.ConnectionClass);
            return (IDbConnection)Activator.CreateInstance(type, connectionString);
        }

        //private string GetConnectionClass()
        //{
        //    if (dbms == "ora")
        //        return "Oracle.DataAccess.Client.OracleConnection";

        //    return provider.ConnectionClass;
        //}
    }
}