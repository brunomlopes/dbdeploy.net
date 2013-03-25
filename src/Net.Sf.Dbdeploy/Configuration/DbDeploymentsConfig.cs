namespace Net.Sf.Dbdeploy.Configuration
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a configuration for a set of deployments.
    /// </summary>
    public class DbDeploymentsConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DbDeploymentsConfig" /> class.
        /// </summary>
        public DbDeploymentsConfig()
        {
            this.Deployments = new List<DbDeployConfig>();
        }

        /// <summary>
        /// Gets or sets the deployments to be executed.
        /// </summary>
        /// <value>
        /// The deployments.
        /// </value>
        public List<DbDeployConfig> Deployments { get; set; }
    }
}
