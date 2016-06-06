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

        public DbmsFactory(string dbms, string connectionString)
        {
            this.dbms = dbms;
            this.connectionString = connectionString;

            this.providers = new DbProviderFile().LoadProviders();
        }

        public virtual IDbmsSyntax CreateDbmsSyntax()
        {
            switch (this.dbms)
            {
				case "db2":
					return new Db2DbmsSyntax();
                case "ora":
                    return new OracleDbmsSyntax();
                case "mssql":
                    return new MsSqlDbmsSyntax();
                case "mysql":
                    return new MySqlDbmsSyntax();
                default:
                    throw new ArgumentException("Supported dbms: db2, ora, mssql, mysql");
            }
        }

        public virtual IDbConnection CreateConnection()
        {
            DatabaseProvider provider = this.providers.GetProvider(dbms);

            Assembly assembly = Assembly.Load(provider.AssemblyName);
            Type type = assembly.GetType(provider.ConnectionClass);

            return (IDbConnection)Activator.CreateInstance(type, this.connectionString);
        }
    }
}