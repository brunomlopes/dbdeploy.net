namespace Net.Sf.Dbdeploy.Database
{
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    using Moq;

    using Net.Sf.Dbdeploy.Scripts;

    using NUnit.Framework;

    /// <summary>
    /// Unit tests for <see cref="DatabaseSchemaVersionManager" /> class.
    /// </summary>
    [TestFixture]
    public class DatabaseSchemaVersionManagerTest
    {
        private readonly ChangeScript script = new ChangeScript("Alpha", 99, "Some Description");

        private DatabaseSchemaVersionManager schemaVersionManager;

        private Mock<IDataReader> expectedResultSet;
        private Mock<QueryExecuter> queryExecuter;
        private Mock<IDbmsSyntax> syntax;

        private string changeLogTableName;

        private List<string> executedQueries;

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

            this.executedQueries = new List<string>();
            this.queryExecuter
                .Setup(e => e.ExecuteQuery(It.IsAny<string>(), It.IsAny<object[]>()))
                .Returns(this.expectedResultSet.Object)
                .Callback<string, object[]>((q, a) => this.executedQueries.Add(q));

            var checkForChangeLogDataReader = new Mock<IDataReader>();
            checkForChangeLogDataReader
                .Setup(r => r.Read())
                .Returns(true);

            this.queryExecuter
                .Setup(e => e.ExecuteQuery(It.Is<string>(v => v.Contains("INFORMATION_SCHEMA")), It.Is<string>(s => s.Equals(changeLogTableName))))
                .Returns(() => checkForChangeLogDataReader.Object);

            this.schemaVersionManager = new DatabaseSchemaVersionManager(this.queryExecuter.Object, this.syntax.Object, changeLogTableName, true);
        }

        [Test]
        public void ShouldUseQueryExecuterToReadInformationFromTheChangelogTable() 
        {
            var readResults = new List<bool> { true, true, true, false };
            var readEnumerator = readResults.GetEnumerator();
            var getResults = new List<ChangeEntry>
                                 {
                                     new ChangeEntry("Alpha", 5)
                                         {
                                             ChangeId = 1,
                                             FileName = "05.test.sql",
                                             Status = ScriptStatus.Success,
                                             Output = "Complete"
                                         }, 
                                     new ChangeEntry("Alpha", 9)
                                         {
                                             ChangeId = 2,
                                             FileName = "09.test.sql",
                                             Status = ScriptStatus.ProblemResolved,
                                             Output = "Fixed"
                                         }, 
                                     new ChangeEntry("Alpha", 12)
                                         {
                                             ChangeId = 3,
                                             FileName = "12.test.sql",
                                             Status = ScriptStatus.Failure,
                                             Output = "Failed"
                                         }, 
                                 };
            var getEnumerator = getResults.GetEnumerator();

            this.expectedResultSet.Setup(rs => rs.Read()).Returns(() =>
            {
                readEnumerator.MoveNext();
                getEnumerator.MoveNext();
                return readEnumerator.Current;
            });
            this.expectedResultSet.Setup(rs => rs["Folder"]).Returns(() => getEnumerator.Current.Folder);
            this.expectedResultSet.Setup(rs => rs["ScriptNumber"]).Returns(() => (short)getEnumerator.Current.ScriptNumber);
            this.expectedResultSet.Setup(rs => rs["ChangeId"]).Returns(() => getEnumerator.Current.ChangeId);
            this.expectedResultSet.Setup(rs => rs["FileName"]).Returns(() => getEnumerator.Current.FileName);
            this.expectedResultSet.Setup(rs => rs["Status"]).Returns(() => (byte)getEnumerator.Current.Status);
            this.expectedResultSet.Setup(rs => rs["Output"]).Returns(() => getEnumerator.Current.Output);

            var changes = this.schemaVersionManager.GetAppliedChanges().ToList();
        
            Assert.AreEqual(3, changes.Count, "Incorrect number of changes found.");
            for (int i = 0; i < getResults.Count; i++)
            {
                AssertChangeProperties(getResults[i], changes[i]);
            }
        }

        [Test]
        public void ShouldUpdateChangelogTable() 
        {
            this.syntax.Setup(s => s.CurrentUser).Returns("DBUSER");
            this.syntax.Setup(s => s.CurrentTimestamp).Returns("TIMESTAMP");

            this.schemaVersionManager.RecordScriptStatus(this.script, ScriptStatus.Success, "Script output");
            string expected = @"INSERT INTO ChangeLog (Folder, ScriptNumber, FileName, StartDate, CompleteDate, AppliedBy, Status, Output) VALUES (@1, @2, @3, TIMESTAMP, TIMESTAMP, DBUSER, @4, @5) 
SELECT ChangeId FROM ChangeLog WHERE Folder = @1 and ScriptNumber = @2";

            Assert.AreEqual(expected, this.executedQueries.FirstOrDefault(), "The query executed was incorrect.");

            this.queryExecuter.Verify(e => e.ExecuteQuery(expected, this.script.Folder, this.script.ScriptNumber, this.script.FileName, (int)ScriptStatus.Success, "Script output"), Times.Once());
        }

        [Test]
        public void ShouldGenerateSqlStringToDeleteChangelogTableAfterUndoScriptApplication() 
        {
            string sql = this.schemaVersionManager.GetChangelogDeleteSql(this.script);
            string expected = "DELETE FROM ChangeLog WHERE Folder = 'Alpha' AND ScriptNumber = 99";

            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void ShouldGetAppliedChangesFromSpecifiedChangelogTableName()
        {
            changeLogTableName = "user_specified_changelog";

            var schemaVersionManagerWithDifferentTableName =
                new DatabaseSchemaVersionManager(this.queryExecuter.Object, this.syntax.Object, changeLogTableName, true);

            schemaVersionManagerWithDifferentTableName.GetAppliedChanges();

            this.queryExecuter.Verify(e => e.ExecuteQuery(It.Is<string>(s => s.StartsWith("SELECT ChangeId, Folder, ScriptNumber, FileName, Status, Output FROM user_specified_changelog"))));
        }

        [Test]
        public void ShouldGenerateSqlStringContainingSpecifiedChangelogTableNameOnDelete() 
        {
            var schemaVersionManagerWithDifferentTableName =
                new DatabaseSchemaVersionManager(this.queryExecuter.Object, this.syntax.Object, "user_specified_changelog", true);

            string updateSql = schemaVersionManagerWithDifferentTableName.GetChangelogDeleteSql(this.script);

            Assert.IsTrue(updateSql.StartsWith("DELETE FROM user_specified_changelog "));
        }

        /// <summary>
        /// Asserts the change properties.
        /// </summary>
        /// <param name="expected">The expected.</param>
        /// <param name="retrieved">The retrieved.</param>
        private static void AssertChangeProperties(ChangeEntry expected, ChangeEntry retrieved)
        {
            Assert.AreEqual(expected.Folder, retrieved.Folder, "Folder does not match.");
            Assert.AreEqual(expected.ScriptNumber, retrieved.ScriptNumber, "ScriptNumber does not match.");
            Assert.AreEqual(expected.ChangeId, retrieved.ChangeId, "ChangeId does not match.");
            Assert.AreEqual(expected.FileName, retrieved.FileName, "FileName does not match.");
            Assert.AreEqual(expected.Status, retrieved.Status, "Status does not match.");
            Assert.AreEqual(expected.Output, retrieved.Output, "Output does not match.");
        }
    }
}