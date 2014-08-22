using System.Collections.Generic;

namespace Net.Sf.Dbdeploy.Scripts
{
    /// <summary>
    /// Provider interface for finding <see cref="ChangeScript"/>s that can be applied.
    /// </summary>
    public interface IAvailableChangeScriptsProvider
    {
        /// <summary>
        /// Gets the available change scripts.
        /// </summary>
        /// <returns>List of change scripts.</returns>
        ICollection<ChangeScript> GetAvailableChangeScripts();
    }
}
