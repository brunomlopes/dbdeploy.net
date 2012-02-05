using System.IO;
using Net.Sf.Dbdeploy.Database;
using NUnit.Framework;
using System;

namespace Net.Sf.Dbdeploy.Console
{
    [TestFixture]
    public class ParserTest
    {
        private readonly DbDeployer dbDeploy = new DbDeployer();
        private readonly Parser parser = new Parser();

        [Test]
        public void CanParseConnectionStringFromCommandLine() 
        {
            parser.Parse("-c \"DataSource:.\\SQLEXPRESS;...;\"".Split(' '), dbDeploy);
            Assert.AreEqual("DataSource:.\\SQLEXPRESS;...;", dbDeploy.ConnectionString);
        }

        [Test]
        public void ThisIsntReallyATestBecuaseThereIsNoAssertButItsVeryUsefulToLookAtTheResult() 
        {
            parser.PrintUsage();
        }

        [Test]
        public void CheckAllOfTheOtherFieldsParseOkHere() 
        {
            parser.Parse(
                ("-c \"DataSource:.\\SQLEXPRESS;...;\" " +
                "--scriptdirectory . -o output.sql " +
                "--changeLogTableName my-change-log " +
                "--dbms ora " +
                "--templatedir /tmp/mytemplates " +
                "--delimiter \\ --delimitertype row").Split(' '), 
                dbDeploy);

            Assert.AreEqual("DataSource:.\\SQLEXPRESS;...;", dbDeploy.ConnectionString);
            Assert.AreEqual(Environment.CurrentDirectory, dbDeploy.ScriptDirectory.FullName);
            Assert.AreEqual("output.sql", dbDeploy.OutputFile.Name);
            Assert.AreEqual("ora", dbDeploy.Dbms);
            Assert.AreEqual("my-change-log", dbDeploy.ChangeLogTableName);
            Assert.AreEqual("\\", dbDeploy.Delimiter);
            Assert.IsInstanceOfType(typeof(RowDelimiter), dbDeploy.DelimiterType);
            Assert.IsTrue(dbDeploy.TemplateDir.FullName.EndsWith(Path.DirectorySeparatorChar + "tmp" + Path.DirectorySeparatorChar + "mytemplates"));
        }

        [Test]
        public void DelimiterTypeWorksOk() 
        {
            parser.Parse("--delimitertype normal".Split(' '), dbDeploy);
            Assert.IsInstanceOfType(typeof(NormalDelimiter), dbDeploy.DelimiterType);

            parser.Parse("--delimitertype row".Split(' '), dbDeploy);
            Assert.IsInstanceOfType(typeof(RowDelimiter), dbDeploy.DelimiterType);
        }

        [Test]
        public void LineEndingWorksOk() 
        {
            Assert.AreEqual(LineEnding.Platform, dbDeploy.LineEnding);

            parser.Parse("--lineending cr".Split(' '), dbDeploy);
            Assert.AreEqual(LineEnding.Cr, dbDeploy.LineEnding);

            parser.Parse("--lineending crlf".Split(' '), dbDeploy);
            Assert.AreEqual(LineEnding.CrLf, dbDeploy.LineEnding);

            parser.Parse("--lineending lf".Split(' '), dbDeploy);
            Assert.AreEqual(LineEnding.Lf, dbDeploy.LineEnding);

            parser.Parse("--lineending platform".Split(' '), dbDeploy);
            Assert.AreEqual(LineEnding.Platform, dbDeploy.LineEnding);

        }
    }
}
