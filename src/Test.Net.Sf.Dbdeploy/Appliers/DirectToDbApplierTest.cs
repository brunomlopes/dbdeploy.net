namespace Net.Sf.Dbdeploy.Appliers
{
    using System.Collections.Generic;
    using System.Data;
    using System.Text;

    using Moq;

    using Net.Sf.Dbdeploy.Database;
    using Net.Sf.Dbdeploy.Exceptions;
    using Net.Sf.Dbdeploy.Scripts;

    using NUnit.Framework;

    class DirectToDbApplierTest
    {
        private Mock<QueryExecuter> queryExecuter;
        private Mock<DatabaseSchemaVersionManager> schemaVersionManager;
        private Mock<QueryStatementSplitter> splitter;

        private DirectToDbApplierAccessor applier;

        [SetUp]
        public void SetUp()
        {
            IDbmsSyntax syntax = null;
            QueryExecuter nullExecuter = null;

            var factory = new Mock<DbmsFactory>("mssql", string.Empty);
            factory.Setup(f => f.CreateConnection()).Returns(new Mock<IDbConnection>().Object);
            factory.Setup(f => f.CreateDbmsSyntax()).Returns(syntax);

            this.queryExecuter = new Mock<QueryExecuter>(factory.Object);

            this.schemaVersionManager = new Mock<DatabaseSchemaVersionManager>(nullExecuter, syntax, "empty");

            this.splitter = new Mock<QueryStatementSplitter>();

            this.applier = new DirectToDbApplierAccessor(
                this.queryExecuter.Object,
                this.schemaVersionManager.Object,
                this.splitter.Object,
                syntax, 
                "ChangeLog",
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
                
            ChangeScript script = new StubChangeScript(1, "script", "split; content");
            
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
            ChangeScript changeScript = new ChangeScript("Scripts", 1, "script.sql");

            this.applier.RecordScriptStatus(changeScript, ScriptStatus.Success, "Script completed");

            this.schemaVersionManager.Verify(s => s.RecordScriptStatus(changeScript, ScriptStatus.Success, "Script completed"));
        }

        [Test]
        public void ShouldRecordFailureInSchemaVersionTable()
        {
            ChangeScript changeScript = new ChangeScript("Scripts", 1, "script.sql");

            this.applier.RecordScriptStatus(changeScript, ScriptStatus.Failure, "Script failed");

            this.schemaVersionManager.Verify(s => s.RecordScriptStatus(changeScript, ScriptStatus.Failure, "Script failed"));
        }

        [Test]
        public void ShouldCommitTransaction() 
        {
            var scripts = new List<ChangeScript> { new StubChangeScript(1, "description", "content") };

            this.queryExecuter.Setup(e => e.BeginTransaction()).Callback(() => { return; });
            this.queryExecuter.Setup(e => e.CommitTransaction()).Callback(() => { return; });

            this.splitter.Setup(s => s.Split(It.IsAny<string>())).Returns<string>(s => new [] { s });

            this.applier.Apply(scripts, false);

            this.queryExecuter.Verify(e => e.BeginTransaction(), Times.Once());
            this.queryExecuter.Verify(e => e.CommitTransaction(), Times.Once());
        }
    }
}