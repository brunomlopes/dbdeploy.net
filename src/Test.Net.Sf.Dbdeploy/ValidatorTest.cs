using System;
using NAnt.Core;
using NUnit.Framework;


namespace Net.Sf.Dbdeploy
{
    [TestFixture]
    public class ValidatorTest
    {
        private static readonly String ConnectionString = "url";
        private static readonly String DBMS = "ora";
        private static readonly String DIR = "dir";
        private static readonly String OUTPUT_FILE = "output file";

        [Test]
        public void ShouldFailWhenPassedNullConnectionString()
        {
            Validator validator = new Validator();
            validator.SetUsage("nant");
            try
            {
                validator.Validate(null, DBMS, DIR, OUTPUT_FILE, null);
                Assert.Fail("BuildException expected");
            }
            catch (BuildException e)
            {
                StringAssert.Contains("connectionString expected", e.Message);
            }
        }

        [Test]
        public void ShouldFailWhenPassedEmptyConnectionString()
        {
            Validator validator = new Validator();
            validator.SetUsage("nant");
            try
            {
                validator.Validate(string.Empty, DBMS, DIR, OUTPUT_FILE, null);
                Assert.Fail("BuildException expected");
            }
            catch (BuildException e)
            {
                StringAssert.Contains("connectionString expected", e.Message);
            }
        }

        [Test]
        public void ShouldFailWhenPassedNonOraDbms()
        {
            Validator validator = new Validator();
            validator.SetUsage("nant");
            try
            {
                validator.Validate(ConnectionString, "nothing", DIR, OUTPUT_FILE, null);
                Assert.Fail("BuildException expected");
            }
            catch (BuildException e)
            {
                StringAssert.Contains("Unknown DBMS", e.Message);
            }
        }

        [Test]
        public void ShouldFailWhenPassedNullDir()
        {
            Validator validator = new Validator();
            validator.SetUsage("nant");
            try
            {
                validator.Validate(ConnectionString, DBMS, null, OUTPUT_FILE, null);
                Assert.Fail("BuildException expected");
            }
            catch (BuildException e)
            {
                StringAssert.Contains("Dir expected", e.Message);
            }
        }

        [Test]
        public void ShouldFailWhenPassedEmptyDir()
        {
            Validator validator = new Validator();
            validator.SetUsage("nant");
            try
            {
                validator.Validate(ConnectionString, DBMS, "", OUTPUT_FILE, null);
                Assert.Fail("BuildException expected");
            }
            catch (BuildException e)
            {
                StringAssert.Contains("Dir expected", e.Message);
            }
        }

        [Test]
        public void ShouldFailWhenPassedNullOutputFile()
        {
            Validator validator = new Validator();
            validator.SetUsage("nant");
            try
            {
                validator.Validate(ConnectionString, DBMS, DIR, null, null);
                Assert.Fail("BuildException expected");
            }
            catch (BuildException e)
            {
                StringAssert.Contains("Output file expected", e.Message);
            }
        }

        [Test]
        public void ShouldFailWhenPassedEmptyOutputFile()
        {
            Validator validator = new Validator();
            validator.SetUsage("nant");
            try
            {
                validator.Validate(ConnectionString, DBMS, DIR, "", null);
                Assert.Fail("BuildException expected");
            }
            catch (BuildException e)
            {
                StringAssert.Contains("Output file expected", e.Message);
            }
        }

        [Test]
        public void ShouldPassWhenPassedNullConnectionStringButCurrentDbVersionIsSpecified()
        {
            Validator validator = new Validator();
            validator.SetUsage("nant");
            validator.Validate(null, DBMS, DIR, OUTPUT_FILE, 2);
        }
    }
}