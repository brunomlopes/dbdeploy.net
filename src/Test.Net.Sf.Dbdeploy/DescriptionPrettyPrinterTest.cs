﻿namespace Net.Sf.Dbdeploy
{
    using global::Dbdeploy.Powershell;

    using NUnit.Framework;

    [TestFixture]
    public class DescriptionPrettyPrinterTest
    {
        private DescriptionPrettyPrinter prettyPrinter;

        [SetUp]
        public void Setup()
        {
            prettyPrinter = new DescriptionPrettyPrinter();
        }
        [Test]
        public void NumberShouldBeSplitFromDescription()
        {
            Assert.AreEqual("001 - Description", prettyPrinter.Format("001_Description"));
        }
        
        [Test]
        public void DescriptionShouldBeSplitFromCamelCasing()
        {
            Assert.AreEqual("001 - Description Of Change", prettyPrinter.Format("001_DescriptionOfChange"));
        }
        
        [Test]
        public void UnderscoreShouldBeConvertedToSpace()
        {
            Assert.AreEqual("001 - Description Of Change", prettyPrinter.Format("001_Description_Of_Change"));
        }

        [Test]
        public void DescriptionWithoutNumberShouldStillBeConvertedFromCamelCase()
        {
            Assert.AreEqual("Description Of Change", prettyPrinter.Format("Description_Of_Change"));
        }
    }
}