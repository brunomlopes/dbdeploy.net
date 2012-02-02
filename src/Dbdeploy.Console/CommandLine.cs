using System;
using System.IO;
using Net.Sf.Dbdeploy.Configuration;
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

                var dbDeploy = new Dbdeploy.DbDeployer
                {
                    InfoWriter = Console.Out,
                    Dbms = config.DbType,
                    ConnectionString = config.DbConnectionString,
                    ChangeLogTableName = config.TableName,
                    ScriptDirectory = new DirectoryInfo("."),
                };

                dbDeploy.Go();
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