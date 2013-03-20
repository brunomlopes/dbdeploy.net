using System.Collections.Generic;
using Net.Sf.Dbdeploy.Scripts;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy
{
    using System.Linq;

    using Net.Sf.Dbdeploy.Database;

    [TestFixture]
    public class PrettyPrinterTest
    {
        private PrettyPrinter prettyPrinter;

        [SetUp]
        protected void SetUp()
        {
            prettyPrinter = new PrettyPrinter();
        }

        [Test]
        public void ShouldDisplayNonRangedNumbersAsSeperateEntities()
        {
            var list = this.CreateUniqueChangeList("Scripts", 1, 3, 5);
            Assert.AreEqual("Scripts: 1, 3, 5", prettyPrinter.Format(list));
        }

        [Test]
        public void ShouldDisplayARangeAsSuch()
        {
            var list = this.CreateUniqueChangeList("Scripts", 1, 2, 3, 4, 5);
            Assert.AreEqual("Scripts: 1..5", prettyPrinter.Format(list));
        }

        [Test]
        public void RangesOfTwoAreNotDisplayedAsARange()
        {
            var list = this.CreateUniqueChangeList("Scripts", 1, 2);
            Assert.AreEqual("Scripts: 1, 2", prettyPrinter.Format(list));
        }

        [Test]
        public void ShouldReturnNoneWithAnEmptyList()
        {
            Assert.AreEqual("(none)", prettyPrinter.Format(new List<ChangeEntry>()));
        }

        [Test]
        public void CanDealWithMixtureOfRangesAndNonRanges()
        {
            var list = this.CreateUniqueChangeList("Scripts", 1, 2, 4, 7, 8, 9, 10, 12);
            Assert.AreEqual("Scripts: 1, 2, 4, 7..10, 12", prettyPrinter.Format(list));
        }

        [Test]
        public void CanFormatAChangeScriptList()
        {
            ChangeScript change1 = new ChangeScript("Scripts", 1);
            ChangeScript change3 = new ChangeScript("Scripts", 3);
            List<ChangeScript> list = new List<ChangeScript>();
            list.Add(change1);
            list.Add(change3);

            Assert.AreEqual("Scripts: 1, 3", prettyPrinter.Format(list));
        }

        /// <summary>
        /// Tests that <see cref="PrettyPrinter"/> can handle multiple folders containing scripts.
        /// </summary>
        [Test]
        public void CanHandleMultipleFolders()
        {
            var list = this.CreateUniqueChangeList("v1.0", 4, 5, 6).Concat(this.CreateUniqueChangeList("v2.0", 1, 2));
            Assert.AreEqual("v1.0: 4..6\r\nv2.0: 1, 2", prettyPrinter.Format(list));
        }

        /// <summary>
        /// Creates the unique change list with the specified folder and script numbers.
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <param name="scriptNumbers">The script numbers.</param>
        /// <returns>Change list.</returns>
        private IEnumerable<UniqueChange> CreateUniqueChangeList(string folder, params int[] scriptNumbers)
        {
            return scriptNumbers.Select(n => new ChangeEntry(folder, n));
        }
    }
}