using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Scripts
{
    [TestFixture]
    public class AssemblyScannerTest
    {
        /// <summary>
        /// Tests if <see cref="AssemblyScanner"/> can read files from the specified assemblies.
        /// </summary>
        [Test]
        public void CanReadFilesFromAssembly()
        {
            var writer = new StringWriter();
            var assemblyScanner = new AssemblyScanner(writer, Encoding.UTF8, new List<Assembly> { Assembly.GetAssembly(GetType())});

            var changeScripts = assemblyScanner.GetChangeScripts();
            
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
        /// <param name="folder">The fileName key.</param>
        /// <param name="scriptNumber">The script number.</param>
        /// <param name="fileName">Name of the file.</param>
        private void VerifyChangeScript(IEnumerable<ChangeScript> changeScripts, string folder, int scriptNumber, string fileName)
        {
            var script = changeScripts.FirstOrDefault(c => c.ScriptName == fileName);
            Assert.IsNotNull(script, "'{0}' script was not found.", fileName);
            Assert.AreEqual(folder, script.Folder, "Folder was incorrect for '{0}'.", fileName);
            Assert.AreEqual(scriptNumber, script.ScriptNumber, "ScriptNumber was incorrect for '{0}'.", fileName);

            var embeddedFileInfo = script.EmbeddedFileInfo;
            Assert.AreEqual(embeddedFileInfo.Assembly, TestAssembly(), "EmbeddedFileInfo.Assembly was incorrect for '{0}'.", fileName);
            Assert.IsNotNull(embeddedFileInfo, "EmbeddedFileInfo should not be null for '{0}'.", fileName);
            Assert.AreEqual(embeddedFileInfo.FileName, fileName, "EmbeddedFileInfo.FileName was incorrect for '{0}'.", fileName);
            Assert.AreEqual(embeddedFileInfo.Folder, folder, "EmbeddedFileInfo.Folder was incorrect for '{0}'.", fileName);
            Assert.AreEqual(embeddedFileInfo.ResourceName, ResourceNameForScript(fileName), "EmbeddedFileInfo.ResourceName was incorrect for '{0}'.", fileName);
        }

        private string ResourceNameForScript(string fileName)
        {
            return TestAssembly().GetManifestResourceNames().First(r => r.EndsWith(fileName));
        }

        private Assembly TestAssembly()
        {
            return Assembly.GetAssembly(GetType());
        }
    }
}