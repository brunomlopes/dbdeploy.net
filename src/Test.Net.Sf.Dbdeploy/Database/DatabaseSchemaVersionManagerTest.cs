using System.Collections.Generic;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Database
{
    [TestFixture]
    public class DatabaseSchemaVersionManagerTest
    {
        [Test]
        public void IfCurrentDbVersionIsSpecifiedShouldGenerateAppliedChangesUpToAndIncludingCurrentDbVersion()
        {
            List<int> appliedChanges = new DatabaseSchemaVersionManager(null, "Main", 10).GetAppliedChangeNumbers();
            Assert.AreEqual(10, appliedChanges.Count);
            Assert.AreEqual(1, appliedChanges[0]);
            Assert.AreEqual(10, appliedChanges[9]);
        }
    }
}
