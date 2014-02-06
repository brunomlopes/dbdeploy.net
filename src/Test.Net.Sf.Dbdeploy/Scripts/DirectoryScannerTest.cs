namespace Net.Sf.Dbdeploy.Scripts
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using NUnit.Framework;

    /// <summary>
    /// Tests for <see cref="DirectoryScanner"/> class.
    /// </summary>
    [TestFixture]
    public class DirectoryScannerTest
    {
        /// <summary>
        /// Tests if <see cref="DirectoryScanner"/> can read files from the specified directory and it's sub-directories.
        /// </summary>
        [Test]
        public void CanReadFilesFromSubDirectories()
        {
            var writer = new StringWriter();
            var directoryScanner = new DirectoryScanner(writer, Encoding.UTF8, new DirectoryInfo(@"Mocks\Versioned"));
            
            List<ChangeScript> changeScripts = directoryScanner.GetChangeScripts();
            
            Assert.IsNotNull(changeScripts, "Change scripts should not be null.");
            Assert.Greater(changeScripts.Count, 0, "No change scripts where found.");

            VerifyChangeScript(changeScripts, "2.0.0.0", 8, "8.Create Product Table.sql");
            VerifyChangeScript(changeScripts, "2.0.0.0", 9, "09.Add Product Data.sql");
            VerifyChangeScript(changeScripts, "2.0.0.0", 10, "10.Add Sold Column.sql");
            VerifyChangeScript(changeScripts, "v2.0.10.0", 1, "1.SQLCMD Add Customer Table.sql");
            VerifyChangeScript(changeScripts, "v2.0.10.0", 2, "2.SQLCMD Add Email Column Table.sql");
            VerifyChangeScript(changeScripts, "v2.0.10.0", 3, "3.SQLCMD Add Customer Data.sql");
        }

        /// <summary>
        /// Verifies the change script information.
        /// </summary>
        /// <param name="changeScripts">The change scripts.</param>
        /// <param name="folder">The folder key.</param>
        /// <param name="scriptNumber">The script number.</param>
        /// <param name="fileName">Name of the file.</param>
        private static void VerifyChangeScript(IEnumerable<ChangeScript> changeScripts, string folder, int scriptNumber, string fileName)
        {
            var script = changeScripts.FirstOrDefault(c => c.ScriptName == fileName);
            Assert.IsNotNull(script, "'{0}' script was not found.", fileName);
            Assert.IsNotNull(script.FileInfo, "FileInfo should not be null for '{0}'.", fileName);
            Assert.AreEqual(fileName, script.FileInfo.Name, "FileInfo was incorrect for '{0}'.", fileName);
            Assert.AreEqual(folder, script.Folder, "Folder was incorrect for '{0}'.", fileName);
            Assert.AreEqual(scriptNumber, script.ScriptNumber, "ScriptNumber was incorrect for '{0}'.", fileName);
        }
    }
}