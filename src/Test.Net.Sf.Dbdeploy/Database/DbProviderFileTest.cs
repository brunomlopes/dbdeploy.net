using System.Linq;

namespace Net.Sf.Dbdeploy.Database
{
    using System.IO;
    using NUnit.Framework;

    [TestFixture]
    public class DbProviderFileTest
    {
        [Test]
        public void ShouldLoadFromAssemblyResourceIfProviderPathIsNull()
        {
            DbProviderFile providerFile = new DbProviderFile();
            Assert.IsNull(providerFile.Path);
            Assert.IsNotNull(providerFile.LoadProviders());
        }
        
        [Test]
        public void ShouldLoadFromFileIfItExists()
        {
            if (File.Exists(DbProviderFile.ProviderFilename))
            {
                File.Delete(DbProviderFile.ProviderFilename);
                File.WriteAllText(DbProviderFile.ProviderFilename,@"
<providers>
  <provider
      name='bogus'
      description='Bogus test'
      assemblyName='System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
      connectionClass='System.Data.SqlClient.SqlConnection'
    />
</providers>
");
            }
            try
            {
                var providerFile = new DbProviderFile();
                Assert.IsNull(providerFile.Path);
                var providers = providerFile.LoadProviders();
                Assert.IsNotNull(providers);
                Assert.Contains("bogus", providers.Items.Select(dp => dp.Name).ToList());
            }
            finally
            {
                File.Delete(DbProviderFile.ProviderFilename);
            }
        }
        
        [Test]
        public void ShouldLoadFromAssemblyResourceIfProviderPathIsUntouched()
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