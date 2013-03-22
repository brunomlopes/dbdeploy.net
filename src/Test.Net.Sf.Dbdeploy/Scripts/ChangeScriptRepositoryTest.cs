namespace Net.Sf.Dbdeploy.Scripts
{
    using System.Collections.Generic;
    using System.Linq;

    using Net.Sf.Dbdeploy.Exceptions;

    using NUnit.Framework;

    [TestFixture]
    public class ChangeScriptRepositoryTest
    {
        [Test]
        public void TestGivenASetOfChangeScriptsReturnsThemCorrectly()
        {
            ChangeScript one = new ChangeScript("Scripts", 1);
            ChangeScript two = new ChangeScript("Scripts", 2);
            ChangeScript three = new ChangeScript("Scripts", 3);
            ChangeScript four = new ChangeScript("Scripts", 4);

            ChangeScript[] scripts = {three, two, four, one};

            ChangeScriptRepository repository = new ChangeScriptRepository(new List<ChangeScript>(scripts));

            List<ChangeScript> list = repository.GetAvailableChangeScripts().ToList();

            Assert.AreEqual(4, list.Count);
            Assert.AreSame(one, list[0]);
            Assert.AreSame(two, list[1]);
            Assert.AreSame(three, list[2]);
            Assert.AreSame(four, list[3]);
        }

        [Test]
        public void TestThrowsWhenConstructedWithAChangeScriptListThatHasDuplicates()
        {
            var two = new ChangeScript("Alpha", 2);
            var three = new ChangeScript("Alpha", 3);
            var four = new ChangeScript("Beta", 3);
            var anotherTwo = new ChangeScript("Alpha", 2);

            try
            {
                var scripts = new[] { three, four, two, anotherTwo };
                // ReSharper disable ObjectCreationAsStatement
                new ChangeScriptRepository(new List<ChangeScript>(scripts));
                // ReSharper restore ObjectCreationAsStatement
                Assert.Fail("expected exception");
            }
            catch (DuplicateChangeScriptException ex)
            {
                Assert.AreEqual("There is more than one change script with key 'Alpha/2'.", ex.Message);
            }
        }

        /// <summary>
        /// Tests if <see cref="DirectoryScanner"/> can order files correctly when the script number does not contain leading zeros.
        /// </summary>
        [Test]
        public void CanOrderFilesWithoutLeadingZeros()
        {
            //StringWriter writer = new StringWriter();
            //DirectoryScanner directoryScanner = new DirectoryScanner(writer, Encoding.UTF8);

            //List<ChangeScript> changeScripts = directoryScanner.GetChangeScriptsForDirectory(new DirectoryInfo(@"Mocks\Versioned\2.0.0.0"));

            //Assert.IsNotNull(changeScripts, "Change scripts should not be null.");
            //Assert.Greater(changeScripts.Count, 0, "No change scripts where found.");

            //Assert.IsTrue(changeScripts.Any(c => "8.Create Product Table.sql"), changeScripts[0].FileName, "8.Create Product Table.sql should be first.");
            //Assert.AreEqual("09.Add Product Data.sql", changeScripts[1].FileName, "09.Add Product Data.sql should be second.");
            //Assert.AreEqual("10.Add Sold Column.sql", changeScripts[2].FileName, "10.Add Sold Column.sql should be last.");

            //Assert.AreEqual(3, changeScripts.Count, "More scripts where found than expected.");
        }

        [Test]
        public void TestChangeScriptsMayBeNumberedFromZero()
        {
            ChangeScript zero = new ChangeScript("Scripts", 0);
            ChangeScript four = new ChangeScript("Scripts", 4);


            ChangeScript[] scripts = new ChangeScript[] {zero, four};
            ChangeScriptRepository repository =
                new ChangeScriptRepository(new List<ChangeScript>(scripts));

            List<ChangeScript> list = repository.GetAvailableChangeScripts().ToList();

            Assert.AreEqual(2, list.Count);
            Assert.AreSame(zero, list[0]);
            Assert.AreSame(four, list[1]);
        }
    }
}