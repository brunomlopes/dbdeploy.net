using System.Collections.Generic;
using FluentAssertions;
using Moq;
using Net.Sf.Dbdeploy.Database;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Scripts
{
    [TestFixture]
    public class RepositorioScriptsTeste
    {
        private RepositorioScripts repositorioScripts;
        private Mock<IAvailableChangeScriptsProvider> changeScriptRepository;
        private Mock<IDatabaseSchemaVersionManager> databaseSchemaVersionManager;

        [SetUp]
        public void SetUp()
        {
            databaseSchemaVersionManager = new Mock<IDatabaseSchemaVersionManager>();
            changeScriptRepository = new Mock<IAvailableChangeScriptsProvider>();
            repositorioScripts = new RepositorioScripts(databaseSchemaVersionManager.Object,changeScriptRepository.Object );
        }

        [Test]
        public void obter_scripts_pedente_execucao()
        {
            var scriptAplicado1 = new ChangeEntry("2.0.0.0", 8);
            scriptAplicado1.ScriptName = "8.Create Product Table.sql";
            scriptAplicado1.Status = ScriptStatus.Success;
            
            var scriptAplicado2 = new ChangeEntry("2.0.0.0", 9);
            scriptAplicado2.ScriptName = "09.Add Product Data.sql";
            scriptAplicado2.Status = ScriptStatus.Success;
            
            databaseSchemaVersionManager.Setup(x => x.GetAppliedChanges()).Returns(new List<ChangeEntry>
            {
                scriptAplicado1,
                scriptAplicado2
            });

            var changeScript1 = new ChangeScript("2.0.0.0", 8, "8.Create Product Table.sql");
            var changeScript2 = new ChangeScript("2.0.0.0", 9, "09.Add Product Data.sql");
            var changeScript3 = new ChangeScript("2.10.0.0", 1, "1.SQLCMD Add Customer Table.sql");

            changeScriptRepository.Setup(x => x.GetAvailableChangeScripts()).Returns(new List<ChangeScript>
            {
                changeScript1,
                changeScript2,
                changeScript3
            });

            var listaChangeScripts = repositorioScripts.ObterScriptsPendenteExecucao(null);

            listaChangeScripts.Should().NotBeEmpty();
            listaChangeScripts.Count.Should().Be(1);

            listaChangeScripts.Should().NotContain(x => x.ScriptName == "8.Create Product Table.sql" && x.Folder == "2.0.0.0");
            listaChangeScripts.Should().NotContain(x => x.ScriptName == "09.Add Product Data.sql" && x.Folder == "2.0.0.0");
            listaChangeScripts.Should().Contain(x => x.ScriptName == "1.SQLCMD Add Customer Table.sql" && x.Folder == "2.10.0.0");
        }

        [Test]
        public void obter_scripts_pedente_execucao_e_marcados_como_resolvido()
        {
            var scriptAplicado1 = new ChangeEntry("2.0.0.0", 8);
            scriptAplicado1.ScriptName = "8.Create Product Table.sql";
            scriptAplicado1.Status = ScriptStatus.ProblemResolved;
            
            var scriptAplicado2 = new ChangeEntry("2.0.0.0", 9);
            scriptAplicado2.ScriptName = "09.Add Product Data.sql";
            scriptAplicado2.Status = ScriptStatus.Success;

            databaseSchemaVersionManager.Setup(x => x.GetAppliedChanges()).Returns(new List<ChangeEntry>
            {
                scriptAplicado1,
                scriptAplicado2
            });

            var changeScript1 = new ChangeScript("2.0.0.0", 8, "8.Create Product Table.sql");
            var changeScript2 = new ChangeScript("2.0.0.0", 9, "09.Add Product Data.sql");
            var changeScript3 = new ChangeScript("2.10.0.0", 1, "1.SQLCMD Add Customer Table.sql");

            changeScriptRepository.Setup(x => x.GetAvailableChangeScripts()).Returns(new List<ChangeScript>
            {
                changeScript1,
                changeScript2,
                changeScript3
            });

            var listaChangeScripts = repositorioScripts.ObterScriptsPendenteExecucao(null);

            listaChangeScripts.Should().NotBeEmpty();
            listaChangeScripts.Count.Should().Be(2);

            listaChangeScripts.Should().NotContain(x => x.ScriptName == "09.Add Product Data.sql" && x.Folder == "2.0.0.0");
            listaChangeScripts.Should().Contain(x => x.ScriptName == "8.Create Product Table.sql" && x.Folder == "2.0.0.0");
            listaChangeScripts.Should().Contain(x => x.ScriptName == "1.SQLCMD Add Customer Table.sql" && x.Folder == "2.10.0.0");
        }

        [Test]
        public void obter_todos_scripts()
        {
            var changeScript1 = new ChangeScript("2.0.0.0", 8, "8.Create Product Table.sql");
            var changeScript2 = new ChangeScript("2.0.0.0", 9, "09.Add Product Data.sql");
            var changeScript3 = new ChangeScript("2.10.0.0", 1, "1.SQLCMD Add Customer Table.sql");

            changeScriptRepository.Setup(x => x.GetAvailableChangeScripts()).Returns(new List<ChangeScript>
            {
                changeScript1,
                changeScript2,
                changeScript3
            });

            var listaTodosOsScripts = repositorioScripts.ObterTodosOsScripts();

            listaTodosOsScripts.Should().NotBeEmpty();
            listaTodosOsScripts.Count.Should().BeGreaterOrEqualTo(3);

            listaTodosOsScripts.Should().Contain(x => x.ScriptName == "8.Create Product Table.sql" && x.Folder == "2.0.0.0");
            listaTodosOsScripts.Should().Contain(x => x.ScriptName == "09.Add Product Data.sql" && x.Folder == "2.0.0.0");
            listaTodosOsScripts.Should().Contain(x => x.ScriptName == "1.SQLCMD Add Customer Table.sql" && x.Folder == "2.10.0.0");
        }

        [Test]
        public void obter_scripts_aplicados()
        {
            var scriptAplicado1 = new ChangeEntry("2.0.0.0", 8);
            scriptAplicado1.ScriptName = "8.Create Product Table.sql";

            var scriptAplicado2 = new ChangeEntry("2.0.0.0", 9);
            scriptAplicado2.ScriptName = "09.Add Product Data.sql";

            databaseSchemaVersionManager.Setup(x => x.GetAppliedChanges()).Returns(new List<ChangeEntry>
            {
                scriptAplicado1,
                scriptAplicado2
            });

            var scriptsAplicados = repositorioScripts.ObterScriptsAplicados();

            scriptsAplicados.Should().NotBeEmpty();
            scriptsAplicados.Count.Should().Be(2);
            scriptsAplicados.Should().Contain(x => x.ScriptName == "8.Create Product Table.sql" && x.Folder == "2.0.0.0");
            scriptsAplicados.Should().Contain(x => x.ScriptName == "09.Add Product Data.sql" && x.Folder == "2.0.0.0");
        }

        [Test]
        public void obter_scripts_com_erro()
        {
            var scriptAplicado1 = new ChangeEntry("2.0.0.0", 8);
            scriptAplicado1.ScriptName = "8.Create Product Table.sql";
            scriptAplicado1.Status = ScriptStatus.Failure;
            scriptAplicado1.Output = "Ocorreu algum erro ao executar";

            var scriptAplicado2 = new ChangeEntry("2.0.0.0", 9);
            scriptAplicado2.ScriptName = "09.Add Product Data.sql";
            scriptAplicado2.Status = ScriptStatus.Failure;
            scriptAplicado2.Output = "Insert duplicate ID error";

            var scriptAplicado3 = new ChangeEntry("2.0.0.0", 10);
            scriptAplicado3.ScriptName = "10.Add Product Data.sql";
            scriptAplicado3.Status = ScriptStatus.Success;

            databaseSchemaVersionManager.Setup(x => x.GetAppliedChanges()).Returns(new List<ChangeEntry>
            {
                scriptAplicado1,
                scriptAplicado2,
                scriptAplicado3
            });

            var scripsComErroDeExecucao = repositorioScripts.ObterScritpsExecutadosComErro();

            scripsComErroDeExecucao.Should().NotBeEmpty();
            scripsComErroDeExecucao.Count.Should().Be(2);
            scripsComErroDeExecucao[0].ScriptName.Should().Be("8.Create Product Table.sql");
            scripsComErroDeExecucao[0].Output.Should().Be("Ocorreu algum erro ao executar");
            scripsComErroDeExecucao[1].ScriptName.Should().Be("09.Add Product Data.sql");
            scripsComErroDeExecucao[1].Output.Should().Be("Insert duplicate ID error");
        }

        [Test]
        public void aplicar_mudancas_ate_script_informado()
        {
            var scriptAplicado1 = new ChangeEntry("2.0.0.0", 8);
            scriptAplicado1.ScriptName = "8.Create Product Table.sql";
            scriptAplicado1.Status = ScriptStatus.Success;
            
            var scriptAplicado2 = new ChangeEntry("2.0.0.0", 9);
            scriptAplicado2.ScriptName = "09.Add Product Data.sql";
            scriptAplicado2.Status = ScriptStatus.Success;

            databaseSchemaVersionManager.Setup(x => x.GetAppliedChanges()).Returns(new List<ChangeEntry>
            {
                scriptAplicado1,
                scriptAplicado2
            });

            var changeScript1 = new ChangeScript("2.0.0.0", 8);
            var changeScript2 = new ChangeScript("2.0.0.0", 9);
            var changeScript3 = new ChangeScript("2.10.0.0", 1);
            var changeScript4 = new ChangeScript("2.10.0.0", 2);
            var changeScript5 = new ChangeScript("2.10.0.0", 3);

            changeScriptRepository.Setup(x => x.GetAvailableChangeScripts()).Returns(new List<ChangeScript>
            {
                changeScript1,
                changeScript2,
                changeScript3,
                changeScript4,
                changeScript5
            });

            var pendenteExecucao = repositorioScripts.ObterScriptsPendenteExecucao(new UniqueChange("2.10.0.0", 2));

            pendenteExecucao.Should().NotBeEmpty();
            pendenteExecucao.Count.Should().Be(2);
            pendenteExecucao[0].Folder.Should().Be("2.10.0.0");
            pendenteExecucao[0].ScriptNumber.Should().Be(1);
            pendenteExecucao[1].Folder.Should().Be("2.10.0.0");
            pendenteExecucao[1].ScriptNumber.Should().Be(2);
        }
    }
}
