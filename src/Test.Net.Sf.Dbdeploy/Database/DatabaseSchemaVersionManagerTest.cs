using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Moq;
using NUnit.Framework;
using Net.Sf.Dbdeploy.Scripts;

namespace Net.Sf.Dbdeploy.Database
{
    [TestFixture]
    public class DatabaseSchemaVersionManagerTest
    {
        private readonly ChangeScript script = new ChangeScript(99, "Some Description");

        private DatabaseSchemaVersionManager schemaVersionManager;

        private Mock<IDataReader> expectedResultSet;
        private Mock<QueryExecuter> queryExecuter;
        private Mock<IDbmsSyntax> syntax;

        private string changeLogTableName;

        [SetUp]
        public void SetUp() 
        {
            this.changeLogTableName = "ChangeLog";

            this.expectedResultSet = new Mock<IDataReader>();

            var connection = new Mock<IDbConnection>();

            var factory = new Mock<DbmsFactory>("mssql", string.Empty);
            factory.Setup(f => f.CreateConnection()).Returns(connection.Object);
        
            this.queryExecuter = new Mock<QueryExecuter>(factory.Object);

            this.syntax = new Mock<IDbmsSyntax>();

            this.queryExecuter
                .Setup(e => e.ExecuteQuery(It.IsAny<string>()))
                .Returns(this.expectedResultSet.Object);

            var checkForChangeLogDataReader = new Mock<IDataReader>();
            checkForChangeLogDataReader
                .Setup(r => r.Read())
                .Returns(true);

            this.queryExecuter
                .Setup(e => e.ExecuteQuery(It.Is<string>(v => v.Contains("INFORMATION_SCHEMA")), It.Is<string>(s => s.Equals(changeLogTableName))))
                .Returns(() => checkForChangeLogDataReader.Object);

            this.schemaVersionManager = new DatabaseSchemaVersionManager(this.queryExecuter.Object, this.syntax.Object, changeLogTableName);
        }

        [Test]
        public void ShouldUseQueryExecuterToReadInformationFromTheChangelogTable() 
        {
            var readResults = new[] { true, true, true, false };
            var readEnumerator = readResults.GetEnumerator();

            this.expectedResultSet.Setup(rs => rs.Read()).Returns(() =>
            {
                readEnumerator.MoveNext();
                return (bool)readEnumerator.Current;
            });
        
            var getResults = new[] { 5, 9, 12 };
            var getEnumerator = getResults.GetEnumerator();
        
            this.expectedResultSet.Setup(rs => rs.GetValue(0)).Returns(() =>
            {
                getEnumerator.MoveNext();
                return (int)getEnumerator.Current;
            });

            List<int> numbers = this.schemaVersionManager.GetAppliedChanges().ToList();
        
            Assert.Contains(5, numbers);
            Assert.Contains(9, numbers);
            Assert.Contains(12, numbers);
        }

        [Test]
        public void ShouldUpdateChangelogTable() 
        {
            this.syntax.Setup(s => s.GenerateUser()).Returns("DBUSER");
            this.syntax.Setup(s => s.GenerateTimestamp()).Returns("TIMESTAMP");

            this.schemaVersionManager.RecordScriptApplied(this.script);
            string expected = "INSERT INTO ChangeLog (ScriptNumber, CompleteDate, AppliedBy, FileName) VALUES (@1, TIMESTAMP, DBUSER, @2)";

            this.queryExecuter.Verify(e => e.Execute(expected, this.script.GetId(), this.script.GetDescription()), Times.Once());
        }

        [Test]
        public void ShouldGenerateSqlStringToDeleteChangelogTableAfterUndoScriptApplication() 
        {
            string sql = this.schemaVersionManager.GetChangelogDeleteSql(this.script);
            string expected = "DELETE FROM ChangeLog WHERE ScriptNumber = 99";

            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void ShouldGetAppliedChangesFromSpecifiedChangelogTableName()
        {
            changeLogTableName = "user_specified_changelog";

            var schemaVersionManagerWithDifferentTableName =
                new DatabaseSchemaVersionManager(this.queryExecuter.Object, this.syntax.Object, changeLogTableName);

            schemaVersionManagerWithDifferentTableName.GetAppliedChanges();

            this.queryExecuter.Verify(e => e.ExecuteQuery(It.Is<string>(s => s.StartsWith("SELECT ScriptNumber FROM user_specified_changelog "))));
        }

        [Test]
        public void ShouldGenerateSqlStringContainingSpecifiedChangelogTableNameOnDelete() 
        {
            var schemaVersionManagerWithDifferentTableName =
                new DatabaseSchemaVersionManager(this.queryExecuter.Object, this.syntax.Object, "user_specified_changelog");

            string updateSql = schemaVersionManagerWithDifferentTableName.GetChangelogDeleteSql(this.script);

            Assert.IsTrue(updateSql.StartsWith("DELETE FROM user_specified_changelog "));
        }
    }
}