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
            Assert.IsNotNull(providerFile.LoadProviders());
        }

        [Test, ExpectedException(typeof (FileNotFoundException))]
        public void ShouldNotInitializeInvalidPath()
        {
            DbProviderFile providerFile = new DbProviderFile();
            providerFile.Path = "bogus path";
            providerFile.LoadProviders();
        }
    }
}