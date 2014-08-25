using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using Net.Sf.Dbdeploy.Appliers;
using Net.Sf.Dbdeploy.Configuration;
using Net.Sf.Dbdeploy.Database;
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
            EnsureTableDoesNotExist("Teste");
            EnsureTableDoesNotExist("Teste2");

            dbDeployConfig = new DbDeployConfig { ConnectionString = @"Server=.\SQLEXPRESS;Initial Catalog=dbdeploy;User Id=sa;Password=sa", Delimiter = "GO" };

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
            {
                executorScriptsIndividuais.Executar(script);
            }

            AssertTableExists("ChangeLog");
            AssertTableExists("Product");
            AssertTableExists("Teste");
            AssertTableExists("Teste2");
        }
    }

    public class ExecutorScriptsIndividuais
    {
        private readonly DbDeployConfig dbDeployConfig;
        private readonly TextWriter textWriter;

        public ExecutorScriptsIndividuais(DbDeployConfig dbDeployConfig, TextWriter textWriter)
        {
            this.dbDeployConfig = dbDeployConfig;
            this.textWriter = textWriter;
        }

        public void Executar(ChangeScript changeScript)
        {
            var dbmsFactory = new DbmsFactory(dbDeployConfig.Dbms, dbDeployConfig.ConnectionString);
            var dbmsSyntax = dbmsFactory.CreateDbmsSyntax();
            var queryExecuter = new QueryExecuter(dbmsFactory);
            var queryStatementSplitter = new QueryStatementSplitter();
            var databaseSchemaVersionManager = new DatabaseSchemaVersionManager(queryExecuter, dbmsSyntax, dbDeployConfig.ChangeLogTableName);
            var directToDbApplier = new DirectToDbApplier(queryExecuter, databaseSchemaVersionManager, queryStatementSplitter, dbmsSyntax, dbDeployConfig.ChangeLogTableName, textWriter);

            var criarChangeLog = CriarTabelaChangeLog(databaseSchemaVersionManager);
            
            directToDbApplier.ApplyChangeScript(changeScript, criarChangeLog);

            queryExecuter.Close();
        }

        private bool CriarTabelaChangeLog(IDatabaseSchemaVersionManager databaseSchemaVersionManager)
        {
            return dbDeployConfig.AutoCreateChangeLogTable && !databaseSchemaVersionManager.ChangeLogTableExists();
        }
    }
}