using System.Collections.Generic;
using Net.Sf.Dbdeploy.Scripts;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy
{
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
            int[] list = {1, 3, 5};
            Assert.AreEqual("1, 3, 5", prettyPrinter.Format(new List<int>(list)));
        }

        [Test]
        public void ShouldDisplayARangeAsSuch()
        {
            int[] list = {1, 2, 3, 4, 5};
            Assert.AreEqual("1..5", prettyPrinter.Format(new List<int>(list)));
        }

        [Test]
        public void RangesOfTwoAreNotDisplayedAsARange()
        {
            int[] list = {1, 2};
            Assert.AreEqual("1, 2", prettyPrinter.Format(new List<int>(list)));
        }

        [Test]
        public void ShouldReturnNoneWithAnEmptyList()
        {
            Assert.AreEqual("(none)", prettyPrinter.Format(new List<int>()));
        }

        [Test]
        public void CanDealWithMixtureOfRangesAndNonRanges()
        {
            int[] list = {1, 2, 4, 7, 8, 9, 10, 12};
            Assert.AreEqual("1, 2, 4, 7..10, 12", prettyPrinter.Format(new List<int>(list)));
        }

        [Test]
        public void CanFormatAChangeScriptList()
        {
            ChangeScript change1 = new ChangeScript(1);
            ChangeScript change3 = new ChangeScript(3);
            List<ChangeScript> list = new List<ChangeScript>();
            list.Add(change1);
            list.Add(change3);

            Assert.AreEqual("1, 3", prettyPrinter.FormatChangeScriptList(list));
        }
    }
}