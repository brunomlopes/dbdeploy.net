namespace Net.Sf.Dbdeploy.Database
{
    using System.Globalization;

    using Net.Sf.Dbdeploy.Scripts;

    /// <summary>
    /// Represents a logged change to the database.
    /// </summary>
    public class ChangeEntry : UniqueChange
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeEntry" /> class.
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <param name="scriptNumber">The script number.</param>
        public ChangeEntry(string folder, int scriptNumber)
            : base(folder, scriptNumber)
        {
        }

        /// <summary>
        /// Gets or sets the change unique ID and primary key in the database.
        /// </summary>
        /// <value>
        /// The change ID.
        /// </value>
        public int ChangeId { get; set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        public string ScriptName { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public ScriptStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the output from the last script run.
        /// </summary>
        /// <value>
        /// The output.
        /// </value>
        public string Output { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}/{1} ({2})", this.Folder, this.ScriptName, ScriptNumber);
        }
    }
}
