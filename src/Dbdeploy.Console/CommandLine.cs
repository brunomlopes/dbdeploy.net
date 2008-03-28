using System;
using System.IO;
using Net.Sf.Dbdeploy.Configuration;
using Net.Sf.Dbdeploy.Database;
using Net.Sf.Dbdeploy.Exceptions;

namespace Net.Sf.Dbdeploy
{
    public class CommandLine
    {
        public static void Main(string[] args)
        {
            try
            {
                IConfiguration config = new ConfigurationFile();
                DbmsFactory factory = new DbmsFactory(config.DbType, config.DbConnectionString);
                DatabaseSchemaVersionManager databaseSchemaVersion = new DatabaseSchemaVersionManager(factory, config.DbDeltaSet, config.CurrentDbVersion, config.TableName);

                new ToPrintStreamDeployer(databaseSchemaVersion, new DirectoryInfo("."), Console.Out, factory.CreateDbmsSyntax(), null).
                    DoDeploy(Int32.MaxValue);
            }
            catch (DbDeployException ex)
            {
                Console.Error.WriteLine(ex.Message);
                Environment.Exit(1);
            }

            catch (Exception ex)
            {
                Console.Error.WriteLine("Failed to apply changes: " + ex);
                Console.Error.WriteLine(ex.StackTrace);
                Environment.Exit(2);
            }
        }
    }
}