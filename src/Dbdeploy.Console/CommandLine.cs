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
            var exitCode = 0;

            try
            {
                // Read arguments from command line
                var deploymentsConfig = OptionsManager.ParseOptions(args);
                var deployer = new DbDeployer();
                foreach (var config in deploymentsConfig.Deployments)
                {
                    deployer.Execute(config, Console.Out);
                }
            }
            catch (UsageException ex)
            {
                Console.Error.WriteLine("ERROR: " + ex.Message);
                
                OptionsManager.PrintUsage();
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