using System;
using NAnt.Core;
using NUnit.Framework;


namespace Net.Sf.Dbdeploy
{
    [TestFixture]
    public class ValidatorTest
    {
        private static readonly String URL = "url";
        private static readonly String DBMS = "ora";
        private static readonly String DIR = "dir";
        private static readonly String OUTPUT_FILE = "output file";

        [Test]
        public void TestShouldFailWhenPassedNullUrl()
        {
            Validator validator = new Validator();
            validator.SetUsage("nant");
            try
            {
                validator.Validate(null, DBMS, DIR, OUTPUT_FILE);
                Assert.Fail("BuildException expected");
            }
            catch (BuildException e)
            {
                StringAssert.Contains("connectionString expected", e.Message);
            }
        }

        [Test]
        public void TestShouldFailWhenPassedEmptyUrl()
        {
            Validator validator = new Validator();
            validator.SetUsage("nant");
            try
            {
                validator.Validate(string.Empty, DBMS, DIR, OUTPUT_FILE);
                Assert.Fail("BuildException expected");
            }
            catch (BuildException e)
            {
                StringAssert.Contains("connectionString expected", e.Message);
            }
        }

        [Test]
        public void TestShouldFailWhenPassedNonOraDbms()
        {
            Validator validator = new Validator();
            validator.SetUsage("nant");
            try
            {
                validator.Validate(URL, "nothing", DIR, OUTPUT_FILE);
                Assert.Fail("BuildException expected");
            }
            catch (BuildException e)
            {
                StringAssert.Contains("Unknown DBMS", e.Message);
            }
        }

        [Test]
        public void TestShouldFailWhenPassedNullDir()
        {
            Validator validator = new Validator();
            validator.SetUsage("nant");
            try
            {
                validator.Validate(URL, DBMS, null, OUTPUT_FILE);
                Assert.Fail("BuildException expected");
            }
            catch (BuildException e)
            {
                StringAssert.Contains("Dir expected", e.Message);
            }
        }

        [Test]
        public void TestShouldFailWhenPassedEmptyDir()
        {
            Validator validator = new Validator();
            validator.SetUsage("nant");
            try
            {
                validator.Validate(URL, DBMS, "", OUTPUT_FILE);
                Assert.Fail("BuildException expected");
            }
            catch (BuildException e)
            {
                StringAssert.Contains("Dir expected", e.Message);
            }
        }

        [Test]
        public void TestShouldFailWhenPassedNullOutputFile()
        {
            Validator validator = new Validator();
            validator.SetUsage("nant");
            try
            {
                validator.Validate(URL, DBMS, DIR, null);
                Assert.Fail("BuildException expected");
            }
            catch (BuildException e)
            {
                StringAssert.Contains("Output file expected", e.Message);
            }
        }

        [Test]
        public void TestShouldFailWhenPassedEmptyOutputFile()
        {
            Validator validator = new Validator();
            validator.SetUsage("nant");
            try
            {
                validator.Validate(URL, DBMS, DIR, "");
                Assert.Fail("BuildException expected");
            }
            catch (BuildException e)
            {
                StringAssert.Contains("Output file expected", e.Message);
            }
        }
    }
}