using System;
using System.IO;
using FluentAssertions;

namespace Net.Sf.Dbdeploy.Appliers
{
    using System.Collections.Generic;
    using System.Data;
    using System.Text;

    using Moq;

    using Database;
    using Exceptions;
    using Scripts;

    using NUnit.Framework;

    class DirectToDbApplierTest
    {
        private Mock<QueryExecuter> queryExecuter;
        private Mock<DatabaseSchemaVersionManager> schemaVersionManager;
        private Mock<QueryStatementSplitter> splitter;

        private DirectToDbApplierAccessor applier;
        private IDbmsSyntax dbmsSyntax;
        private const string ChangeLogTableName = "ChangeLog";

        [SetUp]
        public void SetUp()
        {
            dbmsSyntax = null;
            QueryExecuter nullExecuter = null;

            var factory = new Mock<DbmsFactory>("mssql", string.Empty, null);
            factory.Setup(f => f.CreateConnection()).Returns(new Mock<IDbConnection>().Object);
            factory.Setup(f => f.CreateDbmsSyntax()).Returns(dbmsSyntax);

            this.queryExecuter = new Mock<QueryExecuter>(factory.Object);

            this.schemaVersionManager = new Mock<DatabaseSchemaVersionManager>(nullExecuter, dbmsSyntax, "empty");

            this.splitter = new Mock<QueryStatementSplitter>();

            this.applier = new DirectToDbApplierAccessor(
                this.queryExecuter.Object,
                this.schemaVersionManager.Object,
                this.splitter.Object,
                dbmsSyntax,
                ChangeLogTableName,
                System.Console.Out);
        }

        [Test]
        public void ShouldApplyChangeScriptBySplittingContentUsingTheSplitter()
        {
            this.splitter.Setup(s => s.Split("split; content")).Returns(new List<string> { "split", "content" });

            var output = new StringBuilder();
            this.applier.ApplyChangeScript(new StubChangeScript(1, "script", "split; content"), output);

            this.queryExecuter.Verify(e => e.Execute("split", It.IsAny<StringBuilder>()));
            this.queryExecuter.Verify(e => e.Execute("content", It.IsAny<StringBuilder>()));
        }

        [Test]
        public void ShouldRethrowSqlExceptionsWithInformationAboutWhatStringFailed()
        {
            this.splitter.Setup(s => s.Split("split; content")).Returns(new List<string> { "split", "content" });

            var script = new StubChangeScript(1, "script", "split; content");

            this.queryExecuter.Setup(e => e.Execute("split", It.IsAny<StringBuilder>())).Throws(new DummyDbException());

            try
            {
                var output = new StringBuilder();
                this.applier.ApplyChangeScript(script, output);

                Assert.Fail("exception expected");
            }
            catch (ChangeScriptFailedException e)
            {
                Assert.AreEqual("split", e.ExecutedSql);
                Assert.AreEqual(script, e.Script);
            }

            this.queryExecuter.Verify(e => e.Execute("content"), Times.Never());
        }

        [Test]
        public void ShouldRecordSuccessInSchemaVersionTable()
        {
            var changeScript = new ChangeScript("Scripts", 1, "script.sql");

            this.applier.RecordScriptStatus(changeScript, ScriptStatus.Success, "Script completed");

            this.schemaVersionManager.Verify(s => s.RecordScriptStatus(changeScript, ScriptStatus.Success, "Script completed"));
        }

        [Test]
        public void ShouldRecordFailureInSchemaVersionTable()
        {
            var changeScript = new ChangeScript("Scripts", 1, "script.sql");

            this.applier.RecordScriptStatus(changeScript, ScriptStatus.Failure, "Script failed");

            this.schemaVersionManager.Verify(s => s.RecordScriptStatus(changeScript, ScriptStatus.Failure, "Script failed"));
        }

        [Test]
        public void ShouldCommitTransaction()
        {
            var scripts = new List<ChangeScript> { new StubChangeScript(1, "description", "content") };

            this.queryExecuter.Setup(e => e.BeginTransaction()).Callback(() => { return; });
            this.queryExecuter.Setup(e => e.CommitTransaction()).Callback(() => { return; });

            this.splitter.Setup(s => s.Split(It.IsAny<string>())).Returns<string>(s => new[] { s });

            this.applier.Apply(scripts, false);

            this.queryExecuter.Verify(e => e.BeginTransaction(), Times.Once());
            this.queryExecuter.Verify(e => e.CommitTransaction(), Times.Once());
        }

        [Test]
        public void executar_um_arquivo_de_script_por_vez()
        {
            var dbmsSyntax2 = new Mock<IDbmsSyntax>();
            var changeScript = new Mock<ChangeScript>("Scripts", 1, new FileInfo("script.sql"), Encoding.UTF8);
            dbmsSyntax2.Setup(x => x.CreateChangeLogTableSqlScript(It.IsAny<string>())).Returns("ScriptCreateChangeLog");
            splitter.Setup(s => s.Split(It.IsAny<string>())).Returns<string>(s => new[] { s });
            const string scriptParaExecutar = "script para executar";
            changeScript.Setup(x => x.GetContent()).Returns(scriptParaExecutar);

            var directToDbApplier = new DirectToDbApplier(queryExecuter.Object, schemaVersionManager.Object, splitter.Object, dbmsSyntax2.Object, ChangeLogTableName, System.Console.Out);
            directToDbApplier.ApplyChangeScript(changeScript.Object, true);

            schemaVersionManager.Verify(s => s.RecordScriptStatus(changeScript.Object, ScriptStatus.Started, It.IsAny<string>()), Times.Once);
            queryExecuter.Verify(x => x.BeginTransaction(), Times.Once);
            queryExecuter.Verify(x => x.Execute(scriptParaExecutar, It.IsAny<StringBuilder>()), Times.Once);
            queryExecuter.Verify(e => e.CommitTransaction(), Times.Once());
            schemaVersionManager.Verify(s => s.RecordScriptStatus(changeScript.Object, ScriptStatus.Success, It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void ao_lancar_excecao_quando_executar_script_deve_gravar_status_de_erro_na_changelog()
        {
            var dbmsSyntax2 = new Mock<IDbmsSyntax>();
            var changeScript = new Mock<ChangeScript>("Scripts", 1, new FileInfo("script.sql"), Encoding.UTF8);
            dbmsSyntax2.Setup(x => x.CreateChangeLogTableSqlScript(It.IsAny<string>())).Returns("ScriptCreateChangeLog");
            splitter.Setup(s => s.Split(It.IsAny<string>())).Returns<string>(s => new[] { s });
            changeScript.Setup(x => x.GetContent()).Returns("conteudo do script");
            queryExecuter.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<StringBuilder>())).Throws(new DummyDbException());

            var directToDbApplier = new DirectToDbApplier(queryExecuter.Object, schemaVersionManager.Object, splitter.Object, dbmsSyntax2.Object, ChangeLogTableName, System.Console.Out);
            directToDbApplier.Invoking(x => x.ApplyChangeScript(changeScript.Object, false)).ShouldThrow<Exception>();

            schemaVersionManager.Verify(s => s.RecordScriptStatus(changeScript.Object, ScriptStatus.Started, It.IsAny<string>()), Times.Once);
            queryExecuter.Verify(x => x.BeginTransaction(), Times.Once);
            queryExecuter.Verify(x => x.Execute(It.IsAny<string>(), It.IsAny<StringBuilder>()), Times.Once);
            schemaVersionManager.Verify(s => s.RecordScriptStatus(changeScript.Object, ScriptStatus.Failure, It.IsAny<string>()), Times.Once);
            queryExecuter.Verify(x => x.CommitTransaction(), Times.Once);
        }

        [Test]
        public void executar_script_passando_conteudo()
        {
            const string conteudoSql = "Create Table tabelaTeste (id int not null, name varchar(45) not null, primary key (id));";
            var changeScript = new ChangeScript("1.0.0.0", 1);
            splitter.Setup(s => s.Split(It.IsAny<string>())).Returns<string>(s => new[] { s });

            var directToDbApplier = new DirectToDbApplier(queryExecuter.Object, schemaVersionManager.Object, splitter.Object, new Mock<IDbmsSyntax>().Object, ChangeLogTableName, System.Console.Out);
            directToDbApplier.ApplyScriptContent(changeScript, conteudoSql, true);

            queryExecuter.Verify(x => x.Execute(conteudoSql, It.IsAny<StringBuilder>()), Times.Once);
        }

        [Test]
        public void atualizar_status_do_script_para_sucess_revised_user()
        {
            const string conteudoSql = "Create Table tabelaTeste (id int not null, name varchar(45) not null, primary key (id));";
            var changeScript = new ChangeScript("1.0.0.0", 1);
            splitter.Setup(s => s.Split(It.IsAny<string>())).Returns<string>(s => new[] { s });

            var directToDbApplier = new DirectToDbApplier(queryExecuter.Object, schemaVersionManager.Object, splitter.Object, new Mock<IDbmsSyntax>().Object, ChangeLogTableName, System.Console.Out);
            directToDbApplier.ApplyScriptContent(changeScript, conteudoSql, true);

            schemaVersionManager.Verify(x => x.RecordScriptStatus(changeScript, ScriptStatus.SucessRevisedUser, It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void criar_tabela_changelog()
        {
            const string conteudoSql = "Create Table tabelaTeste (id int not null, name varchar(45) not null, primary key (id));";
            var changeScript = new ChangeScript("1.0.0.0", 1);
            splitter.Setup(s => s.Split(It.IsAny<string>())).Returns<string>(s => new[] { s });
            dbmsSyntax = new MsSqlDbmsSyntax();
            var changeLogTableSqlScript = dbmsSyntax.CreateChangeLogTableSqlScript(ChangeLogTableName);

            var directToDbApplier = new DirectToDbApplier(queryExecuter.Object, schemaVersionManager.Object, splitter.Object, dbmsSyntax, ChangeLogTableName, System.Console.Out);
            directToDbApplier.ApplyScriptContent(changeScript, conteudoSql, true);

            schemaVersionManager.Verify(x => x.RecordScriptStatus(changeScript, ScriptStatus.SucessRevisedUser, It.IsAny<string>()), Times.Once);
            queryExecuter.Verify(x => x.Execute(changeLogTableSqlScript), Times.Once);
        }

        [Test]
        public void nao_criar_tabela_changelog()
        {
            const string conteudoSql = "Create Table tabelaTeste (id int not null, name varchar(45) not null, primary key (id));";
            var changeScript = new ChangeScript("1.0.0.0", 1);
            splitter.Setup(s => s.Split(It.IsAny<string>())).Returns<string>(s => new[] { s });
            dbmsSyntax = new MsSqlDbmsSyntax();
            var changeLogTableSqlScript = dbmsSyntax.CreateChangeLogTableSqlScript(ChangeLogTableName);

            var directToDbApplier = new DirectToDbApplier(queryExecuter.Object, schemaVersionManager.Object, splitter.Object, dbmsSyntax, ChangeLogTableName, System.Console.Out);
            directToDbApplier.ApplyScriptContent(changeScript, conteudoSql, false);

            schemaVersionManager.Verify(x => x.RecordScriptStatus(changeScript, ScriptStatus.SucessRevisedUser, It.IsAny<string>()), Times.Once);
            queryExecuter.Verify(x => x.Execute(changeLogTableSqlScript), Times.Never());
        }
    }
}