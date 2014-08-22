using Net.Sf.Dbdeploy.Appliers;

namespace Net.Sf.Dbdeploy
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Moq;

    using Net.Sf.Dbdeploy.Database;
    using Net.Sf.Dbdeploy.Exceptions;
    using Net.Sf.Dbdeploy.Scripts;

    using NUnit.Framework;

    /// <summary>
    /// Tests for <see cref="Controller"/> class.
    /// </summary>
    [TestFixture]
    public class ControllerTest
    {
        /// <summary>
        /// The target of the tests.
        /// </summary>
        private Controller controller;

        /// <summary>
        /// The output of the controller execution.
        /// </summary>
        private StringBuilder output;

        /// <summary>
        /// The applied changes provider for retrieving what changes have been applied to the database.
        /// </summary>
        private Mock<IAppliedChangesProvider> appliedChangesProvider;

        /// <summary>
        /// The available change scripts provider for retrieving what changes are on the file system.
        /// </summary>
        private Mock<IRepositorioScripts> repositorioScripts;

        /// <summary>
        /// Runs the scripts against the database.
        /// </summary>
        private Mock<IChangeScriptApplier> doApplier;

        /// <summary>
        /// The scripts that would have run.
        /// </summary>
        private IList<ChangeScript> runScripts;

        /// <summary>
        /// Sets up any dependencies before each test.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            // Setup default available scripts.
            this.repositorioScripts = new Mock<IRepositorioScripts>();
            this.repositorioScripts.Setup(p => p.ObterTodosOsScripts())
                .Returns(new List<ChangeScript>
                        {
                            new ChangeScript("1.0", 1),
                            new ChangeScript("1.0", 2),
                            new ChangeScript("1.0", 3),
                            new ChangeScript("1.0", 4),
                            new ChangeScript("1.1", 1),
                            new ChangeScript("1.1", 2),
                        });

            // Capture changes that would be run.
            this.runScripts = new List<ChangeScript>();
            this.doApplier = new Mock<IChangeScriptApplier>();
            this.doApplier
                .Setup(a => a.Apply(It.IsAny<IEnumerable<ChangeScript>>(), false))
                .Callback<IEnumerable<ChangeScript>, bool>((l,b) => this.runScripts = l.ToList());

            this.appliedChangesProvider = new Mock<IAppliedChangesProvider>();
            var undoApplier = new Mock<IChangeScriptApplier>();
            this.output = new StringBuilder();
            this.controller = new Controller(this.repositorioScripts.Object, this.appliedChangesProvider.Object, this.doApplier.Object, undoApplier.Object, false, new StringWriter(this.output));
        }

        /// <summary>
        /// Tests that <see cref="Controller" /> throws an error when a previous script run has failed and it is trying to be run again.
        /// </summary>
        [Test]
        [ExpectedException(typeof(PriorFailedScriptException))]
        public void ShouldThrowErrorWhenPreviousScriptRunFailed()
        {
            // Setup script already run.
            this.repositorioScripts
                .Setup(p => p.ObterScriptsAplicados())
                .Returns(new List<ChangeEntry>
                        {
                            new ChangeEntry("1.0", 1) { ScriptName = "1.test.sql", Status = ScriptStatus.Success },
                            new ChangeEntry("1.0", 2) { ScriptName = "2.test.sql", Status = ScriptStatus.Failure }
                        });

            // Execute controller.
            this.controller.ProcessChangeScripts(null);
        }

        /// <summary>
        /// Tests that <see cref="Controller" /> can apply only scripts not run on the database.
        /// </summary>
        [Test]
        public void ShouldApplyScriptsNotRunToDatabase()
        {
            // Setup script already run.
            this.repositorioScripts
                .Setup(p => p.ObterScriptsAplicados())
                .Returns(new List<ChangeEntry>
                        {
                            new ChangeEntry("1.0", 1) { ScriptName = "1.test.sql", Status = ScriptStatus.Success },
                            new ChangeEntry("1.0", 2) { ScriptName = "2.test.sql", Status = ScriptStatus.Success }
                        });

            this.repositorioScripts
               .Setup(p => p.ObterScriptsPendenteExecucao(null))
               .Returns(new List<ChangeScript>
                        {
                            new ChangeScript("1.0", 3),
                            new ChangeScript("1.0", 4) ,
                            new ChangeScript("1.1", 1) ,
                            new ChangeScript("1.1", 2) ,
                        });
            // Execute controller.
            this.controller.ProcessChangeScripts(null);

            // Verify scripts attempted.
            AssertRunScripts(this.runScripts, "1.0/3", "1.0/4", "1.1/1", "1.1/2");
        }

        /// <summary>
        /// Tests that <see cref="Controller" /> will apply scripts marked as failed when force update is set.
        /// </summary>
        [Test]
        public void ShouldApplyScriptsMarkedAsFailedWhenForceUpdateIsSet()
        {
            // Setup script already run.
            this.repositorioScripts
                .Setup(p => p.ObterScriptsAplicados())
                .Returns(new List<ChangeEntry>
                        {
                            new ChangeEntry("1.0", 1) { ScriptName = "1.test.sql", Status = ScriptStatus.Success },
                            new ChangeEntry("1.0", 2) { ScriptName = "2.test.sql", Status = ScriptStatus.Success },
                            new ChangeEntry("1.0", 3) { ScriptName = "3.test.sql", Status = ScriptStatus.Failure }
                        });

            this.repositorioScripts
               .Setup(p => p.ObterScriptsPendenteExecucao(null))
               .Returns(new List<ChangeScript>
                        {
                            new ChangeScript("1.0", 3),
                            new ChangeScript("1.0", 4) ,
                            new ChangeScript("1.1", 1) ,
                            new ChangeScript("1.1", 2) ,
                        });

            // Execute controller with force update set to true.
            this.controller.ProcessChangeScripts(null, true);

            // Verify scripts attempted.
            AssertRunScripts(this.runScripts, "1.0/3", "1.0/4", "1.1/1", "1.1/2");
        }

        /// <summary>
        /// Asserts the specified scripts where included in the ones to run in the specified order.
        /// </summary>
        /// <param name="scripts">The scripts.</param>
        /// <param name="expectedKeys">The unique keys.</param>
        private static void AssertRunScripts(IList<ChangeScript> scripts, params string[] expectedKeys)
        {
            Assert.Greater(scripts.Count, 0, "No scripts where found that should run.");

            for (int i = 0; i < expectedKeys.Length; i++)
            {
                Assert.Greater(scripts.Count, i, "More change scripts were expected to run.");
                Assert.AreEqual(expectedKeys[i], scripts[i].UniqueKey, "The expected script '{0}' was not next.", expectedKeys[i]);
            }

            Assert.AreEqual(expectedKeys.Length, scripts.Count, "More scripts where applied than should have been.");
        }
    }
}
