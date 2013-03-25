namespace Net.Sf.Dbdeploy.Scripts
{
    using System.Globalization;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Represents a SQL script to be executed.
    /// </summary>
    public class ChangeScript : UniqueChange
    {
        /// <summary>
        /// The undo token to search for to find script to pull out as undoing the current changes.
        /// </summary>
        private const string UndoToken = "--//@UNDO";

        /// <summary>
        /// The SQL file encoding.
        /// </summary>
        private readonly Encoding encoding;

        /// <summary>
        /// Gets or sets the change ID if this script has already been run once in the database.
        /// </summary>
        /// <value>
        /// The change ID.
        /// </value>
        public int ChangeId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeScript" /> class.
        /// </summary>
        /// <param name="folder">The version folder.</param>
        /// <param name="scriptNumber">The script number.</param>
        public ChangeScript(string folder, int scriptNumber) 
            : this(folder, scriptNumber, "test")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeScript" /> class.
        /// </summary>
        /// <param name="folder">The version folder.</param>
        /// <param name="scriptNumber">The script number.</param>
        /// <param name="fileInfo">The file.</param>
        /// <param name="encoding">The encoding.</param>
        public ChangeScript(string folder, int scriptNumber, FileInfo fileInfo, Encoding encoding)
            : base(folder, scriptNumber)
        {
            this.FileInfo = fileInfo;
            this.ScriptName = fileInfo.Name;
            this.encoding = encoding;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeScript" /> class.
        /// </summary>
        /// <param name="folder">The version folder.</param>
        /// <param name="scriptNumber">The script number.</param>
        /// <param name="fileName">Name of the file.</param>
        public ChangeScript(string folder, int scriptNumber, string fileName)
            : base(folder, scriptNumber)
        {
            this.FileInfo = null;
            this.ScriptName = fileName;
        }

        /// <summary>
        /// Gets or sets the file info for the script.
        /// </summary>
        /// <value>
        /// The file info.
        /// </value>
        public FileInfo FileInfo { get; protected set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        public string ScriptName { get; protected set; }

        /// <summary>
        /// Gets the SQL file update content.
        /// </summary>
        /// <returns>File content.</returns>
        public virtual string GetContent()
        {
            return this.GetFileContents();
        }

        /// <summary>
        /// Gets the SQL file undo content.
        /// </summary>
        /// <returns>Undo file content.</returns>
        public virtual string GetUndoContent()
        {
            return this.GetFileContents(undo: true);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}/{1} ({2})", this.Folder, this.ScriptName, this.ScriptNumber);
        }

        /// <summary>
        /// Gets the file contents.
        /// </summary>
        /// <param name="undo">if set to <c>true</c> the undo content will be gathered; otherwise standard update content will be gathered.</param>
        /// <returns>File contents.</returns>
        private string GetFileContents(bool undo = false)
        {
            var result = new StringBuilder();

            bool foundUndo = false;

            using (var input = new StreamReader(this.FileInfo.FullName, this.encoding))
            {
                string str;
                while ((str = input.ReadLine()) != null)
                {
                    // Just keep looping until we find the magic "--//@UNDO"
                    if (UndoToken == str.Trim())
                    {
                        foundUndo = true;                        
                        continue;
                    }

                    if (foundUndo == undo)
                    {
                        result.AppendLine(str);
                    }
                }
            }

            return result.ToString();
        }
    }
}