namespace Net.Sf.Dbdeploy.Appliers
{
    using System;
    using System.IO;

    using Net.Sf.Dbdeploy.Database;
    using Net.Sf.Dbdeploy.Exceptions;

    using NUnit.Framework;

    [TestFixture]
    public class TemplateBaseApplierTest
    {
        [Test]
        public void ShouldThrowUsageExceptionWhenTemplateNotFound() 
        {
            var templateDirectory = new DirectoryInfo(".");
            TemplateBasedApplier applier = new TemplateBasedApplier(new NullWriter(), "some_complete_rubbish", null, ";", new NormalDelimiter(), templateDirectory);
                
            try
            {
                applier.Apply(null);
                        
                Assert.Fail("expected exception");
            } 
            catch (UsageException e) 
            {
                Assert.AreEqual(
                    "Could not find template named some_complete_rubbish_apply.vm" + " at " + templateDirectory.FullName + Environment.NewLine
                    + "Check that you have got the name of the database syntax correct.",
                    e.Message);
            }
        }
    }
}
