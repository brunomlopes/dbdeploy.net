namespace Net.Sf.Dbdeploy.Scripts
{
    using System;
    using System.Collections.Generic;

    using Exceptions;

    /// <summary>
    /// Repository for verifying and providing change scripts in correct order.
    /// </summary>
    public class ChangeScriptRepository : IAvailableChangeScriptsProvider
    {
        /// <summary>
        /// The scripts being managed.
        /// </summary>
        private readonly List<ChangeScript> scripts;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeScriptRepository" /> class.
        /// </summary>
        /// <param name="scripts">The scripts.</param>
        public ChangeScriptRepository(List<ChangeScript> scripts)
        {
            this.scripts = scripts;

            scripts.Sort();

            CheckForDuplicateIds(scripts);
        }

        /// <summary>
        /// Gets the available change scripts.
        /// </summary>
        /// <returns>
        /// List of change scripts.
        /// </returns>
        public ICollection<ChangeScript> GetAvailableChangeScripts()
        {
            return new List<ChangeScript>(scripts.AsReadOnly());
        }

        /// <summary>
        /// Checks for duplicate ids in the list of change scripts.
        /// </summary>
        /// <param name="scripts">The scripts.</param>
        /// <exception cref="DuplicateChangeScriptException">There is more than one change script with number  + lastId</exception>
        private static void CheckForDuplicateIds(IEnumerable<ChangeScript> scripts)
        {
            string lastKey = null;

            // Since scripts are ordered, if one following is the same, we know there is a duplicate.
            foreach (var script in scripts)
            {
                if (string.Equals(script.UniqueKey, lastKey, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new DuplicateChangeScriptException(string.Format("There is more than one change script with key '{0}'.", lastKey));
                }

                lastKey = script.UniqueKey;
            }
        }
    }
}