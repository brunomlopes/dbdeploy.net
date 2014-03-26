using System.Collections;
using FluentAssertions;

namespace Net.Sf.Dbdeploy.Database
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Net.Sf.Dbdeploy.Appliers;
    using Net.Sf.Dbdeploy.Scripts;
    using NUnit.Framework;

    [TestFixture]
    public class ScriptGenerationTest
    {
        [Test]
        public void GenerateConsolidatedChangesScriptForAllDatabasesAndCompareAgainstTemplate()
        {
            foreach (string syntax in new[] { "mssql", "mysql", "ora", "firebird", "postgre" }) 
            {
                try 
                {
                    System.Console.WriteLine("Testing syntax {0}\n", syntax);

                    this.RunIntegratedTestAndConfirmOutputResults(syntax, new DirectoryInfo(@".\Resources"));
                }
                catch (Exception e) 
                {
                    throw new ApplicationException("Failed while testing syntax " + syntax, e);
                }
            }
        }

        [Test]
        public void GenerateConsolidatedChangesScriptForAllDatabasesLoadingTemplatesFromResourcesAndCompareAgainstTemplate()
        {
            foreach (string syntax in new[] { "mssql", "mysql", "ora", "firebird", "postgre" })
            {
                try 
                {
                    System.Console.WriteLine("Testing syntax {0}\n", syntax);

                    this.RunIntegratedTestAndConfirmOutputResults(syntax, null);
                }
                catch (Exception e) 
                {
                    throw new ApplicationException("Failed while testing syntax " + syntax, e);
                }
            }
        }

        private void RunIntegratedTestAndConfirmOutputResults(string syntaxName, DirectoryInfo templateDirectory) 
        {
            StringWriter writer = new StringWriter();
            var listReplaces = new List<string>() { "$script.Guid", "$script.Folder", "$script.ScriptNumber", "$script.ScriptName" };
            var listExpecteds = new List<string>()
                {
                    "START CHANGE SCRIPT v1.0/001_change.sql",
                    "END CHANGE SCRIPT v1.0/001_change.sql",
                    "START CHANGE SCRIPT v1.0/002_change.sql",
                    "END CHANGE SCRIPT v1.0/002_change.sql",
                    "INSERT INTO ChangeLog (ChangeId, Folder, ScriptNumber, ScriptName, StartDate, AppliedBy, ScriptStatus, ScriptOutput)",
                    "UPDATE ChangeLog"
                };

            ChangeScript changeOne = new StubChangeScript(1, "001_change.sql", "-- contents of change script 1");
            ChangeScript changeTwo = new StubChangeScript(2, "002_change.sql", "-- contents of change script 2");

            List<ChangeScript> changeScripts = new List<ChangeScript> { changeOne, changeTwo };
            ChangeScriptRepository changeScriptRepository = new ChangeScriptRepository(changeScripts);

            var factory = new DbmsFactory(syntaxName, string.Empty);
            var dbmsSyntax = factory.CreateDbmsSyntax();

            var createChangeLogTable = false;
            StubSchemaManager schemaManager = new StubSchemaManager(dbmsSyntax, createChangeLogTable);

            IChangeScriptApplier applier = new TemplateBasedApplier(writer, dbmsSyntax, "ChangeLog", ";", new NormalDelimiter(), templateDirectory);
            Controller controller = new Controller(changeScriptRepository, schemaManager, applier, null, createChangeLogTable, System.Console.Out);

            controller.ProcessChangeScripts(null);

            var actual = writer.ToString();

            try
            {
                var expected = ReadExpectedFileContents(GetExpectedFilename(syntaxName));
                foreach (var currentExpected in listExpecteds)
                {
                    expected.Should().Contain(currentExpected, string.Format("The expected script does not contain {0}", currentExpected));
                }

                foreach (var replace in listReplaces)
                {
                    if (actual.Contains(replace)) 
                        Assert.Fail(string.Format("A regex from template does not were replaced. \n\rRegex: {0} ", replace));
                }
            }
            catch (Exception)
            {
                // Output actual template on failure.
                Console.WriteLine("\n\nActual Template ({0}):", syntaxName);
                Console.WriteLine(actual);
                throw;
            }
        }

        private String GetExpectedFilename(string dbSyntaxName) 
        {
            return @"Resources\Database\" + dbSyntaxName + "_expected.sql";
        }

        private string ReadExpectedFileContents(string expectedFilename)
        {
            Stream stream = File.OpenRead(expectedFilename);

            StreamReader reader = new StreamReader(stream);

            try 
            {
                return this.ReadEntireStreamIntoAStringWithConversionToSystemDependantLineTerminators(reader);
            }
            finally 
            {
                reader.Dispose();
            }
        }

        private string ReadEntireStreamIntoAStringWithConversionToSystemDependantLineTerminators(TextReader reader)
        {
            StringWriter writer = new StringWriter();
            try 
            {
                string line;

                while ((line = reader.ReadLine()) != null) 
                {
                    writer.WriteLine(line);
                }

                writer.Flush();

                return writer.ToString();
            }
            finally 
            {
                writer.Dispose();
            }
        }

        private class StubSchemaManager : DatabaseSchemaVersionManager 
        {
            private readonly bool changeLogTableExists;

            public StubSchemaManager(IDbmsSyntax syntax, bool changeLogTableExists)
                : base(null, syntax, "ChangeLog")
            {
                this.changeLogTableExists = changeLogTableExists;
            }

            public override bool ChangeLogTableExists()
            {
                return !changeLogTableExists;
            }

            public override IList<ChangeEntry> GetAppliedChanges()
            {
                return new List<ChangeEntry>();
            }
        }
    }
}