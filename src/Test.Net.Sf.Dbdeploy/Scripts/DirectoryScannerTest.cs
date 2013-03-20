using Net.Sf.Dbdeploy.Exceptions;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Scripts
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Tests for <see cref="DirectoryScanner"/> class.
    /// </summary>
    [TestFixture]
    public class DirectoryScannerTest
    {
        /// <summary>
        /// Tests if <see cref="DirectoryScanner"/> can order files correctly when the script number does not contain leading zeros.
        /// </summary>
        [Test]
        public void CanOrderFilesWithoutLeadingZeros()
        {
            StringWriter writer = new StringWriter();
            DirectoryScanner directoryScanner = new DirectoryScanner(writer, Encoding.UTF8);
            
            List<ChangeScript> changeScripts = directoryScanner.GetChangeScriptsForDirectory(new DirectoryInfo(@"Mocks\Versioned\2.0.0.0"));
            
            Assert.IsNotNull(changeScripts, "Change scripts should not be null.");
            Assert.Greater(changeScripts.Count, 0, "No change scripts where found.");

            Assert.AreEqual("8.Create Product Table.sql", changeScripts[0].FileName, "8.Create Product Table.sql should be first.");
            Assert.AreEqual("09.Add Product Data.sql", changeScripts[1].FileName, "09.Add Product Data.sql should be second.");
            Assert.AreEqual("10.Add Sold Column.sql", changeScripts[2].FileName, "10.Add Sold Column.sql should be last.");

            Assert.AreEqual(3, changeScripts.Count, "More scripts where found than expected.");
        }
    }
}