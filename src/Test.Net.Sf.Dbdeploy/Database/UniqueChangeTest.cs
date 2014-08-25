using System.Collections.Generic;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Database
{
    /// <summary>
    /// Tests for <see cref="UniqueChange"/> base class.
    /// </summary>
    [TestFixture]
    public class UniqueChangeTest
    {
        /// <summary>
        /// Tests that <see cref="UniqueChange" /> compares folder then script number.
        /// </summary>
        [Test]
        public void ShouldCompareFolderThenScriptNumber()
        {
            var changeEntries = new List<UniqueChange>
                                    {
                                        new ChangeEntry("Beta", 8), 
                                        new ChangeEntry("Alpha", 3), 
                                        new ChangeEntry("Alpha", 2), 
                                        new ChangeEntry("Beta", 4), 
                                        new ChangeEntry("Gamma", 0),
                                    };
            changeEntries.Sort();

            AssertSequence(changeEntries, "Alpha/2", "Alpha/3", "Beta/4", "Beta/8", "Gamma/0");
        }

        /// <summary>
        /// Tests that <see cref="UniqueChange" /> compares a folder that matches versioning (v1.0.0.0) correctly be numbers.
        /// </summary>
        [Test]
        public void ShouldCompareVersionFolderCorrectly()
        {
            var changeEntries = new List<UniqueChange>
                                    {
                                        new ChangeEntry("v2.0.4.0", 1), 
                                        new ChangeEntry("v10.1.0.0", 2), 
                                        new ChangeEntry("v2.0.10.1", 3), 
                                        new ChangeEntry("2.1.0", 4), 
                                        new ChangeEntry("v2.0", 5),
                                        new ChangeEntry("v1.0", 6)
                                    };
            changeEntries.Sort(); 

            AssertSequence(changeEntries, "v1.0/6", "v2.0/5", "v2.0.4.0/1", "v2.0.10.1/3", "2.1.0/4", "v10.1.0.0/2");
        }

        /// <summary>
        /// Tests that <see cref="UniqueChange" /> can compare versioned and standard folders together.
        /// </summary>
        [Test]
        public void ShouldCompareVersionFolderAndStandardFoldersCorrectly()
        {
            var changeEntries = new List<UniqueChange>
                                    {
                                        new ChangeEntry("v2.0.4.0", 1), 
                                        new ChangeEntry("Zeta", 4), 
                                        new ChangeEntry("v2.0.10.1", 3), 
                                        new ChangeEntry("Alpha", 6), 
                                        new ChangeEntry("v2.0", 5),
                                        new ChangeEntry("Alpha", 2)
                                    };
            changeEntries.Sort();

            AssertSequence(changeEntries, "Alpha/2", "Alpha/6", "v2.0/5", "v2.0.4.0/1", "v2.0.10.1/3", "Zeta/4");
        }

        /// <summary>
        /// Asserts the sequence is as specified.
        /// </summary>
        /// <param name="changeEntries">The change entries.</param>
        /// <param name="uniqueKeys">The unique keys.</param>
        private void AssertSequence(IList<UniqueChange> changeEntries, params string[] uniqueKeys)
        {
            for (int i = 0; i < uniqueKeys.Length; i++)
            {
                if (i >= changeEntries.Count)
                {
                    Assert.Fail("There are less entries in sequence than there should be.");
                }

                Assert.AreEqual(uniqueKeys[i], changeEntries[i].UniqueKey, string.Format("Item at position {0} should be '{1}'.", i, uniqueKeys[i]));
            }
        }
    }
}
