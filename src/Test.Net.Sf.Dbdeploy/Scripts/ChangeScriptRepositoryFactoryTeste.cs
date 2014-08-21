using System.IO;
using FluentAssertions;
using Net.Sf.Dbdeploy.Configuration;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Scripts
{
    [TestFixture]
    public class ChangeScriptRepositoryFactoryTeste
    {
        [Test]
        public void retornar_change_script_repository()
        {
            var dbDeployConfig = new DbDeployConfig
            {
                ScriptDirectory = new DirectoryInfo(@"Mocks\Versioned"),
            };
            var changeScriptRepositoryFactory = new ChangeScriptRepositoryFactory(dbDeployConfig, new StringWriter());

            var changeScriptRepository = changeScriptRepositoryFactory.Obter();

            changeScriptRepository.Should().NotBeNull();
            changeScriptRepository.Should().BeOfType<ChangeScriptRepository>();
        }

        [Test]
        public void buscar_scripts_dos_diretorios()
        {
            var dbDeployConfig = new DbDeployConfig
            {
                ScriptDirectory = new DirectoryInfo(@"Mocks\Versioned"),
            };

            var changeScriptRepositoryFactory = new ChangeScriptRepositoryFactory(dbDeployConfig, new StringWriter());

            var changeScriptRepository = changeScriptRepositoryFactory.Obter();

            var availableChangeScripts = changeScriptRepository.GetAvailableChangeScripts();

            availableChangeScripts.Should().NotBeEmpty();
            availableChangeScripts.Count.Should().BeGreaterOrEqualTo(6);
            availableChangeScripts.Should().Contain(x => x.ScriptName == "10.Add Sold Column.sql" && x.Folder == "2.0.0.0");
            availableChangeScripts.Should().Contain(x => x.ScriptName == "2.SQLCMD Add Email Column Table.sql" && x.Folder == "v2.0.10.0");
        }

    }
}
