namespace Net.Sf.Dbdeploy.Console
{
    using System;
    using System.IO;
    using System.Linq;

    using Net.Sf.Dbdeploy.Configuration;
    using Net.Sf.Dbdeploy.Database;

    using NUnit.Framework;

    [TestFixture]
    public class OptionsManagerTest
    {
        [Test]
        public void CanParseConnectionStringFromCommandLine() 
        {
            var config = OptionsManager.ParseOptions("-c \"DataSource:.\\SQLEXPRESS;...;\"".Split(' ')).Deployments.First();
            Assert.AreEqual("DataSource:.\\SQLEXPRESS;...;", config.ConnectionString);
        }

        [Test]
        public void ThisIsntReallyATestBecuaseThereIsNoAssertButItsVeryUsefulToLookAtTheResult() 
        {
            OptionsManager.PrintUsage();
        }

        [Test]
        public void CheckAllOfTheOtherFieldsParseOkHere() 
        {
            var config = OptionsManager.ParseOptions(
                ("-c \"DataSource:.\\SQLEXPRESS;...;\" " +
                "--scriptdirectory . -o output.sql " +
                "--changelogtablename my-change-log " +
                "--dbms ora " +
                "--templatedirectory /tmp/mytemplates " +
                "--delimiter \\ --delimitertype row").Split(' ')).Deployments.First();

            Assert.AreEqual("DataSource:.\\SQLEXPRESS;...;", config.ConnectionString);
            Assert.AreEqual(Environment.CurrentDirectory, config.ScriptDirectory.FullName);
            Assert.AreEqual("output.sql", config.OutputFile.Name);
            Assert.AreEqual("ora", config.Dbms);
            Assert.AreEqual("my-change-log", config.ChangeLogTableName);
            Assert.AreEqual("\\", config.Delimiter);
            Assert.IsInstanceOfType(typeof(RowDelimiter), config.DelimiterType);
            Assert.IsTrue(config.TemplateDirectory.FullName.EndsWith(Path.DirectorySeparatorChar + "tmp" + Path.DirectorySeparatorChar + "mytemplates"));
        }

        [Test]
        public void DelimiterTypeWorksOk() 
        {
            var config = OptionsManager.ParseOptions("--delimitertype normal".Split(' ')).Deployments.First();
            Assert.IsInstanceOfType(typeof(NormalDelimiter), config.DelimiterType);

            config = OptionsManager.ParseOptions("--delimitertype row".Split(' ')).Deployments.First();
            Assert.IsInstanceOfType(typeof(RowDelimiter), config.DelimiterType);
        }

        [Test]
        public void LineEndingWorksOk()
        {
            var config = new DbDeployConfig();
            Assert.AreEqual(DbDeployDefaults.LineEnding, config.LineEnding);

            config = OptionsManager.ParseOptions("--lineending cr".Split(' ')).Deployments.First();
            Assert.AreEqual(LineEnding.Cr, config.LineEnding);

            config = OptionsManager.ParseOptions("--lineending crlf".Split(' ')).Deployments.First();
            Assert.AreEqual(LineEnding.CrLf, config.LineEnding);

            config = OptionsManager.ParseOptions("--lineending lf".Split(' ')).Deployments.First();
            Assert.AreEqual(LineEnding.Lf, config.LineEnding);

            config = OptionsManager.ParseOptions("--lineending platform".Split(' ')).Deployments.First();
            Assert.AreEqual(LineEnding.Platform, config.LineEnding);

        }
    }
}
