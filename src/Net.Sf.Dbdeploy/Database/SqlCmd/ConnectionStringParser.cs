namespace Net.Sf.Dbdeploy.Database.SqlCmd
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Class that parses out connection string values.
    /// </summary>
    public static class ConnectionStringParser
    {
        /// <summary>
        /// Parses the specified connection string into it's components.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>Connection string components.</returns>
        public static ConnectionStringInfo Parse(string connectionString)
        {
            var info = new ConnectionStringInfo();

            var entries = connectionString.Split(';').Where(s => !string.IsNullOrWhiteSpace(s));
            foreach (var entry in entries)
            {
                var pair = entry.Split('=');
                if (pair.Length == 2)
                {
                    var name = pair[0].ToLowerInvariant().Trim();
                    var value = pair[1].Trim();

                    switch (name)
                    {
                        case "server":
                        case "data source":
                        case "datasource":
                            info.Server = value;
                            break;

                        case "database":
                        case "initial catalog":
                            info.Database = value;
                            break;

                        case "uid":
                        case "user":
                        case "user id":
                            info.UserId = value;
                            break;

                        case "pwd":
                        case "password":
                            info.Password = value;
                            break;

                        case "trusted_connection":
                            info.TrustedConnection = value.ToLowerInvariant() == "true";
                            break;
                    }
                }
                else
                {
                    throw new FormatException("The connectionString does not have a correct matching of key value pairs.");
                }
            }

            return info;
        }
    }
}
