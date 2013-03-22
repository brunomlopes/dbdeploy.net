namespace Net.Sf.Dbdeploy
{
    using System;
    using System.IO;

    using Net.Sf.Dbdeploy.Configuration;
    using Net.Sf.Dbdeploy.Exceptions;

    public class CommandLine
    {
        public static void Main(string[] args)
        {
            var dbDeploy = new DbDeployer
            {
                InfoWriter = Console.Out,
                ScriptDirectory = new DirectoryInfo("."),
            };

            try 
            {
                IConfiguration config = new ConfigurationFile();

                dbDeploy.Dbms = config.DbType;
                dbDeploy.ConnectionString = config.DbConnectionString;
                dbDeploy.ChangeLogTableName = config.TableName;
            }
            catch (System.Configuration.ConfigurationException)
            {
                // ignore
            }

            var parser = new Parser();
            var exitCode = 0;

            try
            {
                // Read arguments from command line
                parser.Parse(args, dbDeploy);

                dbDeploy.Go();
            }
            catch (UsageException ex)
            {
                Console.Error.WriteLine("ERROR: " + ex.Message);
                
                parser.PrintUsage();
            }
            catch (DbDeployException ex)
            {
                Console.Error.WriteLine(ex.Message);
                exitCode = 1;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Failed to apply changes: " + ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
                exitCode = 2;
            }

            Environment.Exit(exitCode);
        }
    }
}