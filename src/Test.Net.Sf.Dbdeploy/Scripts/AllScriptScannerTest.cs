using System.Collections.Generic;
using Moq;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Scripts
{
    [TestFixture]
    public class AllScriptScannerTest
    {
        [Test]
        public void ExecutesAllScanners()
        {
            var scannerOne = AScanner();
            var scannerTwo = AScanner();

            var allScriptScanner = new AllScriptScanner(scannerOne.Object, scannerTwo.Object);
            allScriptScanner.GetChangeScripts();

            scannerOne.Verify(s => s.GetChangeScripts());
            scannerTwo.Verify(s => s.GetChangeScripts());
        }

        [Test]
        public void ReturnsAllScannedScripts()
        {
            var scriptOne = new ChangeScript("2.0.0", 1);
            var scriptTwo = new ChangeScript("2.0.0", 2);

            var scannerOne = new Mock<IScriptScanner>();
            scannerOne.Setup(s => s.GetChangeScripts()).Returns(new List<ChangeScript> { scriptOne });

            var scannerTwo = new Mock<IScriptScanner>();
            scannerTwo.Setup(s => s.GetChangeScripts()).Returns(new List<ChangeScript> { scriptTwo });

            var allScriptScanner = new AllScriptScanner(scannerOne.Object, scannerTwo.Object);
            var changeScripts = allScriptScanner.GetChangeScripts();

            var containsFirstScript = changeScripts.Contains(scriptOne);
            Assert.IsTrue(containsFirstScript);

            var containsSecondScript = changeScripts.Contains(scriptTwo);
            Assert.IsTrue(containsSecondScript);
        }

        private Mock<IScriptScanner> AScanner()
        {
            var scannerOne = new Mock<IScriptScanner>();
            scannerOne.Setup(s => s.GetChangeScripts()).Returns(new List<ChangeScript>());

            return scannerOne;
        }
    }
}