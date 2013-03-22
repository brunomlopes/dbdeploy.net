using System.IO;
using System.Text;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Scripts
{
    [TestFixture]
    public class ChangeScriptTest
    {
        [Test]
        public void TestChangeScriptsHaveAnIdAndAFile()
        {
            FileInfo file = new FileInfo("abc.txt");
            ChangeScript changeScript = new ChangeScript("v1.0", 5, file, Encoding.Default);

            Assert.AreEqual(5, changeScript.ScriptNumber);
            Assert.AreEqual(file, changeScript.FileInfo);
        }
        
        [Test]
        public void TestChangeScriptsNaturallyOrderById()
        {
            ChangeScript one = new ChangeScript("v1.0", 1);
            ChangeScript two = new ChangeScript("v1.0", 2);
            Assert.IsTrue(one.CompareTo(two) < 1);
            Assert.IsTrue(two.CompareTo(one) >= 1);
        }

        [Test]
        public void TestToStringReturnsASensibleValue()
        {
            FileInfo file = new FileInfo("abc.txt");
            ChangeScript changeScript = new ChangeScript("v1.0", 5, file, Encoding.Default);
            Assert.AreEqual("v1.0/abc.txt (5)", changeScript.ToString());
        }
    }
}