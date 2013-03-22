namespace Net.Sf.Dbdeploy.Configuration
{
    using System.IO;

    /// <summary>
    /// Interface to manager for handling DbDeploy configuration files.
    /// </summary>
    public interface IDbDeployConfigurationManager
    {
        /// <summary>
        /// Reads the configuration.
        /// </summary>
        /// <param name="xmlFilePath">The XML file path.</param>
        /// <returns>Configuration set.</returns>
        DbDeploymentsConfig ReadConfiguration(string xmlFilePath);

        /// <summary>
        /// Reads the configuration.
        /// </summary>
        /// <param name="xmlFile">The XML file text reader.</param>
        /// <param name="rootPath">The root path to resolve relative paths to.</param>
        /// <returns>
        /// Configuration set.
        /// </returns>
        DbDeploymentsConfig ReadConfiguration(TextReader xmlFile, string rootPath);
    }
}