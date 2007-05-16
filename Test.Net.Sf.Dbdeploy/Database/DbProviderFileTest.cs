using System;
using System.IO;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Database
{
    [TestFixture]
    public class DbProviderFileTest
    {
        [Test]
        public void ShouldUseCurrentWorkingDirByDefault()
        {
            DbProviderFile providerFile = new DbProviderFile();
            string expectedPath = Environment.CurrentDirectory + @"\dbproviders.xml";
            Assert.AreEqual(expectedPath, providerFile.Path);
        }

        [Test]
        public void ShouldLoadFromAssemblyDirectory()
        {
                
        }

        [Test, ExpectedException(typeof(FileNotFoundException))]
        public void ShouldNotInitializeInvalidPath()
        {
            DbProviderFile providerFile = new DbProviderFile();
            providerFile.LoadFrom("bogus path");
        }
    }
}