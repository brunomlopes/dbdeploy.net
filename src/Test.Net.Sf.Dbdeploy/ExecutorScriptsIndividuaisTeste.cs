using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using FluentAssertions;
using Net.Sf.Dbdeploy.Configuration;
using Net.Sf.Dbdeploy.Scripts;
using Net.Sf.Dbdeploy.Utils;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy
{
    [TestFixture]
    public class ExecutorScriptsIndividuaisTeste : DatabaseTestBase
    {
        private ExecutorScriptsIndividuais executorScriptsIndividuais;
        private DbDeployConfig dbDeployConfig;

        public ExecutorScriptsIndividuaisTeste()
            : base(ConfigurationManager.AppSettings["ConnString"])
        { }

        [SetUp]
        public void SetUp()
        {
            EnsureTableDoesNotExist(ChangeLogTableName);
            EnsureTableDoesNotExist("Product");
            EnsureTableDoesNotExist("Customer");
            EnsureTableDoesNotExist("Teste");
            EnsureTableDoesNotExist("Teste2");
            EnsureTableDoesNotExist("tabelaTeste");

            dbDeployConfig = new DbDeployConfig { ConnectionString = @"Server=.\SQLEXPRESS;Initial Catalog=dbdeploy;User Id=sa;Password=sa" };

            executorScriptsIndividuais = new ExecutorScriptsIndividuais(dbDeployConfig, new StringWriter());
        }

        [Test]
        public void executar_uma_lista_de_scripts_individuais()
        {
            var listaChangeScript = new List<ChangeScript>
            {
                new ChangeScript("2.0.0.0", 8, new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Mocks\Versioned\2.0.0.0\8.Create Product Table.sql")), Encoding.UTF8),
                new ChangeScript("3.0.0.0", 1, new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Mocks\Versioned\3.0.0.0\4.SQLCMD Add Tabela de Teste.sql")), Encoding.UTF8),
            };

            foreach (var script in listaChangeScript)
                executorScriptsIndividuais.Executar(script);

            AssertTableExists("ChangeLog");
            AssertTableExists("Product");
            AssertTableExists("Teste");
            AssertTableExists("Teste2");
        }

        [Test]
        public void executar_um_conteudo_script()
        {
            const string conteudoSql = "Create Table tabelaTeste (id int not null, name varchar(45) not null, primary key (id));";
            var changeScript = new ChangeScript("2.0.0.0", 8, new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Mocks\Versioned\2.0.0.0\8.Create Product Table.sql")), Encoding.UTF8);

            executorScriptsIndividuais.Executar(changeScript, conteudoSql);

            AssertTableExists("tabelaTeste");
            AssertTableExists("ChangeLog");
        }

        [Test]
        public void ao_lancar_excecao_deve_retornar_o_motivo_do_erro()
        {
            var listaChangeScript = new List<ChangeScript>
            {
                new ChangeScript("2.0.0.0", 8, new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Mocks\Versioned\2.0.0.0\10.Add Sold Column.sql")), Encoding.UTF8)
            };

            executorScriptsIndividuais.Invoking(x => x.Executar(listaChangeScript[0])).ShouldThrow<Exception>();

            VerificarSeGravouAMensagemDeErroEStatusCorreto();
        }
    }
}