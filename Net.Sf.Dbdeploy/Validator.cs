using System;
using NAnt.Core;

namespace Net.Sf.Dbdeploy
{
    public class Validator
    {
        private String usage;

        private static String ERROR_MESSAGE_HEADER = "Dbdeploy parameter validation error" + Environment.NewLine +
                                                     Environment.NewLine;

        private static String NANT_USAGE = Environment.NewLine + Environment.NewLine + "Dbdeploy NAnt Task Usage"
                                           + Environment.NewLine + "======================="
                                           + Environment.NewLine + Environment.NewLine + "\t<dbdeploy"
                                           + Environment.NewLine + "\t\tconnectionString=\"[DATABASE URL]\" *"
                                           + Environment.NewLine + "\t\tdbms=\"[YOUR DBMS]\" *"
                                           + Environment.NewLine + "\t\tdir=\"[YOUR SCRIPT FOLDER]\" *"
                                           + Environment.NewLine + "\t\toutputfile=\"[OUTPUT SCRIPT PATH + NAME]\" *"
                                           + Environment.NewLine +
                                           "\t\tmaxNumberToApply=\"[NUMBER OF THE LAST SCRIPT TO APPLY]\""
                                           + Environment.NewLine + "\t\tdeltaSet=\"[NAME OF DELTA SET TO BE APPLIED]\""
                                           + Environment.NewLine + "\t\tundoOutputfile=\"[UNDO SCRIPT PATH + NAME]\""
                                           + Environment.NewLine + "\t/>"
                                           + Environment.NewLine + Environment.NewLine +
                                           "* - Indicates mandatory parameter";

        private static String COMMAND_LINE_USAGE = "\n\n\nDbdeploy Command Line Usage"
                                                   + "\n==========================="
                                                   + "\n\n\tTODO - SPECIFY USAGE HERE";

        public void Validate(String connectionString, String dbms, String dir, String outputFile)
        {
            if (connectionString == null || connectionString == "")
            {
                throw new BuildException(ConstructErrorMessage("connectionString expected"));
            }

            if (dbms == null || dbms == "" ||
                !dbms.Equals("ora") && !dbms.Equals("ora-sqlplus") && !dbms.Equals("hsql") && !dbms.Equals("syb-ase") &&
                !dbms.Equals("mssql") && !dbms.Equals("mysql"))
            {
                throw new BuildException(
                    ConstructErrorMessage("Unknown DBMS: " + dbms +
                                          "\n\nAllowed values:\nora - Oracle\nhsql - Hypersonic SQL\nsyb-ase - Sybase ASE\nmssql - Microsoft SQL Server\nmysql - MySQL database"));
            }

            if (dir == null || dir == "")
            {
                throw new BuildException(ConstructErrorMessage("Dir expected"));
            }

            if (outputFile == null || outputFile == "")
            {
                throw new BuildException(ConstructErrorMessage("Output file expected"));
            }
        }

        private String ConstructErrorMessage(String customMessagePortion)
        {
            if (usage == "nant")
            {
                return ERROR_MESSAGE_HEADER + customMessagePortion + NANT_USAGE;
            }
            else if (usage == "commandline")
            {
                return ERROR_MESSAGE_HEADER + customMessagePortion + COMMAND_LINE_USAGE;
            }
            else
            {
                throw new BuildException("Unexpected usage!");
            }
        }

        public void SetUsage(String usage)
        {
            this.usage = usage;
        }
    }
}