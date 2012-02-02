using System;
using System.Collections.Generic;
using Moq;
using Net.Sf.Dbdeploy.Database;
using Net.Sf.Dbdeploy.Exceptions;
using Net.Sf.Dbdeploy.Scripts;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Appliers
{
    class DirectToDbApplierTest
    {
        private readonly Mock<QueryExecuter> queryExecuter;
        private readonly Mock<DatabaseSchemaVersionManager> schemaVersionManager;
        private readonly Mock<QueryStatementSplitter> splitter;

        private DirectToDbApplierAccessor applier;

        public DirectToDbApplierTest()
        {
            this.queryExecuter = new Mock<QueryExecuter>();
            this.schemaVersionManager = new Mock<DatabaseSchemaVersionManager>();
            this.splitter = new Mock<QueryStatementSplitter>();
        }

        [SetUp]
        public void SetUp() 
        {
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