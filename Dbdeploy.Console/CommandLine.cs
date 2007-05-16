using System;
using System.Configuration;
using System.IO;
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
                if (args.Length != 1)
                {
                    Console.Error.WriteLine("usage: dbdeploy propertyfilename");
                    Environment.Exit(3);
                }

                string dbConnectionString = ConfigurationManager.AppSettings["db.connection"];
                string dbType = ConfigurationManager.AppSettings["db.type"];

                string deltaSet = ConfigurationManager.AppSettings["db.deltaSet"];

                DbmsFactory factory = new DbmsFactory(dbType, dbConnectionString);
                DatabaseSchemaVersionManager databaseSchemaVersion = new DatabaseSchemaVersionManager(factory, deltaSet);

                new ToPrintStreamDeployer(databaseSchemaVersion, new DirectoryInfo("."), Console.Out, factory.CreateDbmsSyntax(), null).
                    doDeploy(Int32.MaxValue);
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