namespace Net.Sf.Dbdeploy.Database.SqlCmd
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Text;

    using Net.Sf.Dbdeploy.Appliers;

    using System.Diagnostics;

    /// <summary>
    /// Manager for handling execution and deployment of SQLCMD.
    /// </summary>
    public class SqlCmdExecutor : IDisposable
    {
        /// <summary>
        /// The SQLCMD file name.
        /// </summary>
        private const string SqlCmdFileName = "SQLCMD.EXE";

        /// <summary>
        /// The SQL CMD resource file.
        /// </summary>
        private const string SqlCmdResourceFile = "SQLCMD.rll";

        /// <summary>
        /// The timeout for running SQLCMD.
        /// </summary>
        private const int SqlCmdTimeout = 60000;

        /// <summary>
        /// The error severity for SQLCMD exiting.
        /// </summary>
        private const int ErrorSeverity = 1;

        /// <summary>
        /// The path to deploy to SQLCMD to temporarily to execution.
        /// </summary>
        private static readonly string ExtractPath;

        /// <summary>
        /// The connection string for the database.
        /// </summary>
        private readonly string connectionString;

        /// <summary>
        /// The info text writer to display output.
        /// </summary>
        private readonly TextWriter infoTextWriter;

        /// <summary>
        /// Initializes static members of the <see cref="SqlCmdApplier" /> class.
        /// </summary>
        static SqlCmdExecutor()
        {
            ExtractPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlCmdExecutor" /> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="infoTextWriter">The info text writer.</param>
        public SqlCmdExecutor(string connectionString, TextWriter infoTextWriter)
        {
            this.infoTextWriter = infoTextWriter;
            this.connectionString = connectionString;
            DeploySqlCmd();
        }

        /// <summary>
        /// Executes the SQL script file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns><c>true</c> if the SQL file succeeded; otherwise false.</returns>
        public bool ExecuteFile(FileInfo file)
        {
            bool success;
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = GetResourceFilePath(SqlCmdFileName),
                    Arguments = BuildCommandArguments(this.connectionString, file),
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };

                process.Start();
                process.WaitForExit(SqlCmdTimeout);

                this.infoTextWriter.WriteLine(process.StandardOutput.ReadToEnd());
                this.infoTextWriter.WriteLine(process.StandardError.ReadToEnd());

                success = process.ExitCode == 0;
            }

            return success;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            CleanUpSqlCmd();
        }

        /// <summary>
        /// Extracts the SQL CMD to be able to execute against it.
        /// </summary>
        private static void DeploySqlCmd()
        {
            // Extract SQLCMD and dependencies from resource to the file system so it can be run.
            ExtractFile(SqlCmdFileName);
            ExtractFile(SqlCmdResourceFile);
        }

        /// <summary>
        /// Extracts resource to file.
        /// </summary>
        /// <param name="resourceKey">The resource key.</param>
        private static void ExtractFile(string resourceKey)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = string.Format("Net.Sf.Dbdeploy.Resources.{0}", resourceKey);
            using (Stream resourceStream = assembly.GetManifestResourceStream(resourceName))
            {
                using (var fileStream = new FileStream(GetResourceFilePath(resourceKey), FileMode.Create))
                {
                    resourceStream.CopyTo(fileStream);
                }
            }
        }

        /// <summary>
        /// Cleans up SQLCMD from the file system.
        /// </summary>
        private static void CleanUpSqlCmd()
        {
            DeleteFile(SqlCmdFileName);
            DeleteFile(SqlCmdResourceFile);
        }

        /// <summary>
        /// Deletes the file resource.
        /// </summary>
        /// <param name="resourceKey">The resource key.</param>
        private static void DeleteFile(string resourceKey)
        {
            var filePath = GetResourceFilePath(resourceKey);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        /// <summary>
        /// Gets the resource file path on the local file system.
        /// </summary>
        /// <param name="resourceKey">The resource key.</param>
        /// <returns>Full file path on local file system.</returns>
        private static string GetResourceFilePath(string resourceKey)
        {
            return Path.Combine(ExtractPath, resourceKey);
        }

        /// <summary>
        /// Builds the SQL command arguments.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="scriptFile">The script file to execute.</param>
        /// <returns>
        /// SQLCMD command arguments.
        /// </returns>
        private static string BuildCommandArguments(string connectionString, FileInfo scriptFile)
        {
            var info = ConnectionStringParser.Parse(connectionString);
            var result = new StringBuilder();

            AppendItem(result, "-S \"{0}\"", info.Server);
            AppendItem(result, "-d \"{0}\"", info.Database);
            AppendItem(result, "-U \"{0}\"", info.UserId);
            AppendItem(result, "-P \"{0}\"", info.Password);
            AppendItem(result, "-E", info.TrustedConnection ? "true" : null);
            AppendItem(result, "-i \"{0}\"", scriptFile.FullName);
            AppendItem(result, "-V {0}", ErrorSeverity.ToString(CultureInfo.InvariantCulture));

            return result.ToString();
        }

        /// <summary>
        /// Appends the item if it exists.
        /// </summary>
        /// <param name="output">The result.</param>
        /// <param name="format">The format.</param>
        /// <param name="value">The value.</param>
        private static void AppendItem(StringBuilder output, string format, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                // Add a space if there is something before.
                if (output.Length > 0)
                {
                    output.Append(' ');
                }

                output.AppendFormat(format, value);
            }
        }
    }
}
