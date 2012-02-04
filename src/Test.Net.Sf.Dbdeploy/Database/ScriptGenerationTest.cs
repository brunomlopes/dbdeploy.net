using System;
using System.Collections.Generic;
using System.IO;
using Net.Sf.Dbdeploy.Appliers;
using Net.Sf.Dbdeploy.Scripts;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Database
{
    [TestFixture]
    public class ScriptGenerationTest
    {
        [Test]
        public void GenerateConsolidatedChangesScriptForAllDatabasesAndCompareAgainstTemplate()
        {
            foreach (string syntax in new[] { "mssql", "mysql", "ora" }) 
            {
                try 
                {
                    Console.WriteLine("Testing syntax {0}\n", syntax);

                    this.RunIntegratedTestAndConfirmOutputResults(syntax);
                }
                catch (Exception e) 
                {
                    throw new ApplicationException("Failed while testing syntax " + syntax, e);
                }
            }
        }

        private void RunIntegratedTestAndConfirmOutputResults(string syntaxName) 
        {
            StringWriter writer = new StringWriter();

            ChangeScript changeOne = new StubChangeScript(1, "001_change.sql", "-- contents of change script 1");
            ChangeScript changeTwo = new StubChangeScript(2, "002_change.sql", "-- contents of change script 2");

            List<ChangeScript> changeScripts = new List<ChangeScript> { changeOne, changeTwo };
            ChangeScriptRepository changeScriptRepository = new ChangeScriptRepository(changeScripts);

            var factory = new DbmsFactory(syntaxName, string.Empty);

            StubSchemaManager schemaManager = new StubSchemaManager(factory.CreateDbmsSyntax());

            IChangeScriptApplier applier = new TemplateBasedApplier(writer, syntaxName, "changelog", ";", new NormalDelimiter(), null);
            Controller controller = new Controller(changeScriptRepository, schemaManager, applier, null, Console.Out);

            controller.ProcessChangeScripts(null);

            Assert.AreEqual(this.ReadExpectedFileContents(this.GetExpectedFilename(syntaxName)), writer.ToString());
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
            public StubSchemaManager(IDbmsSyntax syntax) : base(null, syntax, "changelog")
            {
            }

            public override ICollection<int> GetAppliedChanges()
            {
                return new List<int>();
            }
        }
    }
}