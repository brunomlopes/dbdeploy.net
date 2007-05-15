using System;
using System.IO;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Database
{
    [TestFixture]
    public class DbProviderFileTest
    {
        private readonly DbProviderFile providerFile = new DbProviderFile();

        [Test]
        public void ShouldUseCurrentWorkingDirByDefault()
        {
            string expectedPath = Environment.CurrentDirectory + @"\dbproviders.xml";
            Assert.AreEqual(expectedPath, providerFile.Path);
        }

        [Test, ExpectedException(typeof(FileNotFoundException))]
        public void ShouldNotInitializeInvalidPath()
        {
            providerFile.LoadFrom("bogus path");
        }
    }
}