using System;
using System.Globalization;

namespace Net.Sf.Dbdeploy.Database
{
    /// <summary>
    /// Represents any unique change.
    /// </summary>
    public class UniqueChange : IComparable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UniqueChange" /> class.
        /// </summary>
        /// <param name="uniqueKey">The unique key folder and script number combination (Alpha/2).</param>
        /// <exception cref="System.ArgumentNullException">uniqueKey;The unique key must be a supplied.</exception>
        public UniqueChange(string uniqueKey)
        {
            if (string.IsNullOrWhiteSpace(uniqueKey))
            {
                throw new ArgumentNullException("uniqueKey", "The unique key must be a supplied.");
            }

            var parts = uniqueKey.Split('/');
            if (parts.Length != 2)
            {
                throw new ArgumentException("The unique key must be a folder and script number combination (Alpha/2).", "uniqueKey");
            }

            int scriptNumber;
            if (!int.TryParse(parts[1], out scriptNumber))
            {
                throw new ArgumentException("The unique key script number must be an integer (Alpha/2).", "uniqueKey");
            }

            Folder = parts[0];
            ScriptNumber = scriptNumber;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UniqueChange" /> class.
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <param name="scriptNumber">The script number.</param>
        public UniqueChange(string folder, int scriptNumber)
        {
            Folder = folder;
            ScriptNumber = scriptNumber;

            Version version;
            Version.TryParse(folder.TrimStart('v'), out version);
            Version = version;
        }

        /// <summary>
        /// Get the new Guid value for Primary Key constraint
        /// </summary>
        public string Guid
        {
            get { return System.Guid.NewGuid().ToString(); }
        }

        /// <summary>
        /// Gets the folder that contained the script executed (part of unique key).
        /// </summary>
        /// <value>
        /// The folder.
        /// </value>
        public string Folder { get; private set; }

        /// <summary>
        /// Gets the script number executed inside the folder.
        /// </summary>
        /// <value>
        /// The script number.
        /// </value>
        public int ScriptNumber { get; private set; }

        /// <summary>
        /// Gets the version of the folder if in the format of (v1.0.0.0) or (1.0.0).
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public Version Version { get; private set; }

        /// <summary>
        /// Gets the unique key that represents this change on the file system.
        /// </summary>
        /// <value>
        /// The unique key.
        /// </value>
        public string UniqueKey
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}/{1}", Folder, ScriptNumber);
            }
        }

        /// <summary>
        /// Compares the object to another giving a result of less than, equal to, or greater than.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns>Integer value corresponding to less than, equal to, or greater than.</returns>
        public int CompareTo(object obj)
        {
            var other = (UniqueChange)obj;

            // Compare versions if both folders are in version format v1.0.0.0.
            int result;
            if (Version != null && other.Version != null)
            {
                result = Version.CompareTo(other.Version);
            }
            else
            {
                // Compare folder names as is.
                result = string.Compare(Folder, other.Folder, StringComparison.InvariantCultureIgnoreCase);
            }

            // Compare script number of folders are the same.
            if (result == 0)
            {
                result = ScriptNumber.CompareTo(other.ScriptNumber);
            }

            return result;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}/{1}", Folder, ScriptNumber);
        }
    }
}