using System;
using System.Collections.Generic;
using System.Data;
using Moq;
using Net.Sf.Dbdeploy.Database;
using Net.Sf.Dbdeploy.Exceptions;
using Net.Sf.Dbdeploy.Scripts;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Appliers
{
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
                Console.Out);
        }

        [Test]
        public void ShouldApplyChangeScriptBySplittingContentUsingTheSplitter() 
        {
            this.splitter.Setup(s => s.Split("split; content")).Returns(new List<string> { "split", "content" });

            this.applier.ApplyChangeScript(new StubChangeScript(1, "script", "split; content"));

            this.queryExecuter.Verify(e => e.Execute("split"));
            this.queryExecuter.Verify(e => e.Execute("content"));
        }

        [Test]
        public void ShouldRethrowSqlExceptionsWithInformationAboutWhatStringFailed() 
        {
            this.splitter.Setup(s => s.Split("split; content")).Returns(new List<string> { "split", "content" });
                
            ChangeScript script = new StubChangeScript(1, "script", "split; content");
            
            this.queryExecuter.Setup(e => e.Execute("split")).Throws(new DummyDbException());

            try 
            {
                this.applier.ApplyChangeScript(script);
                        
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
        public void ShouldInsertToSchemaVersionTable() 
        {
            ChangeScript changeScript = new ChangeScript(1, "script.sql");

            this.applier.InsertToSchemaVersionTable(changeScript);

            this.schemaVersionManager.Verify(s => s.RecordScriptApplied(changeScript));
        }

        [Test]
        public void ShouldCommitTransaction() 
        {
            var scripts = new List<ChangeScript> { new StubChangeScript(1, "description", "content") };

            this.applier.Apply(scripts);

            this.queryExecuter.Verify(e => e.BeginTransaction(), Times.Once());
            this.queryExecuter.Verify(e => e.CommitTransaction(), Times.Once());
        }
    }
}