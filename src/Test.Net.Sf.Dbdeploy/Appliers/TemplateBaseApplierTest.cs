namespace Net.Sf.Dbdeploy.Appliers
{
    using System;
    using System.IO;

    using Net.Sf.Dbdeploy.Database;
    using Net.Sf.Dbdeploy.Exceptions;

    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class TemplateBaseApplierTest
    {
        [Test]
        public void ShouldThrowUsageExceptionWhenTemplateNotFound() 
        {
            var templateDirectory = new DirectoryInfo(".");
            var mockDbmsSyntax = new Mock<IDbmsSyntax>();
            mockDbmsSyntax.Setup(d => d.GetTemplateFileNameFor("apply"))
                .Returns("some_complete_rubbish_apply.vm");

            TemplateBasedApplier applier = new TemplateBasedApplier(new NullWriter(), mockDbmsSyntax.Object, null, ";", new NormalDelimiter(), templateDirectory);
                
            try
            {
                applier.Apply(null, false);
                        
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
