using System.Collections.Generic;
using System.Linq;
using Net.Sf.Dbdeploy.Exceptions;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Scripts
{
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
            ChangeScript two = new ChangeScript("Scripts", 2);
            ChangeScript three = new ChangeScript("Scripts", 3);
            ChangeScript anotherTwo = new ChangeScript("Scripts", 2);

            try
            {
                ChangeScript[] scripts = {three, two, anotherTwo};
                new ChangeScriptRepository(new List<ChangeScript>(scripts));
                Assert.Fail("expected exception");
            }
            catch (DuplicateChangeScriptException ex)
            {
                Assert.AreEqual("There is more than one change script with number 2", ex.Message);
            }
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