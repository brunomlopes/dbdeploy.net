using System;
using Net.Sf.Dbdeploy.Database;
using Net.Sf.Dbdeploy.Exceptions;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Appliers
{
    [TestFixture]
    public class TemplateBaseApplierTest
    {
        [Test]
        public void ShouldThrowUsageExceptionWhenTemplateNotFound() 
        {
            TemplateBasedApplier applier = new TemplateBasedApplier(new NullWriter(), "some_complete_rubbish", null, ";", new NormalDelimiter(), null);
                
            try
            {
                applier.Apply(null);
                        
                Assert.Fail("expected exception");
            } 
            catch (UsageException e) 
            {
                Assert.AreEqual(
                    "Could not find template named some_complete_rubbish_apply.vm" + Environment.NewLine
                    + "Check that you have got the name of the database syntax correct.",
                    e.Message);
            }
        }
    }
}
