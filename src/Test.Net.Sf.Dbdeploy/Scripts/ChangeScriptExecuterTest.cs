using System;
using System.IO;
using Net.Sf.Dbdeploy.Database;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Scripts
{
    [TestFixture]
    public class ChangeScriptExecuterTest
    {
        private ChangeScript script;

        [SetUp]
        protected void SetUp()
        {
            FileInfo tempFile = new FileInfo(Path.GetTempFileName());
            script = new ChangeScript(1, tempFile);
        }

        [TearDown]
        protected void TearDown()
        {
            script.GetFile().Delete();
        }

    	[Test]
    	public void ShouldInsertBeginAndEndTransactionStatementsForEachDelta()
    	{
			const string deltaScriptContent = @"foo bar";
			using (StreamWriter stream = script.GetFile().CreateText())
				stream.Write(deltaScriptContent);

    		StringWriter writer = new StringWriter();
    		MsSqlDbmsSyntax syntax = new MsSqlDbmsSyntax();
    		ChangeScriptExecuter executer = new ChangeScriptExecuter(writer, syntax, true);
			executer.ApplyChangeDoScript(script);

    		string deltaFragment = writer.ToString();

			Console.Write(deltaFragment);

			string expectedBegin = syntax.GenerateBeginTransaction();
			int startOfBegin = deltaFragment.IndexOf(deltaScriptContent) - expectedBegin.Length - Environment.NewLine.Length;
    		string resultBegin = deltaFragment.Substring(startOfBegin, expectedBegin.Length);

			Assert.AreEqual(expectedBegin, resultBegin);

			string expectedCommit = syntax.GenerateCommitTransaction();
			int startOfCommit = deltaFragment.IndexOf(deltaScriptContent) + deltaScriptContent.Length + Environment.NewLine.Length;
			string resultCommit = deltaFragment.Substring(startOfCommit, expectedCommit.Length);

			Assert.AreEqual(expectedCommit, resultCommit);
		}

    	[Test]
    	public void ShouldNotInsertBeginOrEndTransactionStatementIfPropertyIsNotSet()
    	{
			const string deltaScriptContent = @"foo bar";
			using (StreamWriter stream = script.GetFile().CreateText())
				stream.Write(deltaScriptContent);

			StringWriter writer = new StringWriter();
			MsSqlDbmsSyntax syntax = new MsSqlDbmsSyntax();
			ChangeScriptExecuter executer = new ChangeScriptExecuter(writer, syntax, false);
			executer.ApplyChangeDoScript(script);

			string deltaFragment = writer.ToString();

			Console.Write(deltaFragment);

    		int beingIndex = deltaFragment.IndexOf(syntax.GenerateBeginTransaction());
			Assert.AreEqual(-1, beingIndex);

    		int endIndex = deltaFragment.IndexOf(syntax.GenerateCommitTransaction());
			Assert.AreEqual(-1, endIndex);
		}

        [Test]
        public void ShouldNotOutputUndoScriptIfUndoTagIsMissing()
        {
            using (StreamWriter stream = script.GetFile().CreateText())
            {
                stream.Write(@"IF NOT EXISTS (SELECT * FROM sysobjects WHERE Name= 'nav' AND xType= 'U')
BEGIN

	CREATE TABLE [dbo].[nav](
		[navID] [uniqueidentifier] NOT NULL,
		[code] [nchar](10) NOT NULL,
		[description] [nvarchar](50) NOT NULL,
		[inceptionDate] [datetime] NOT NULL,
		[pricingDate] [datetime] NOT NULL,
		[price] [money] NOT NULL,
		[pennyChange] [money] NOT NULL,
	 CONSTRAINT [PK_navID] PRIMARY KEY CLUSTERED (
		[navID] ASC
	) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]
END");
            }

            StringWriter writer = new StringWriter();
            ChangeScriptExecuter executer = new ChangeScriptExecuter(writer, new MsSqlDbmsSyntax(), false);
            executer.ApplyChangeUndoScript(script);
            Assert.IsFalse(writer.ToString().Contains("CREATE TABLE"), "output should not contain the original script!");
        }
    }
}
