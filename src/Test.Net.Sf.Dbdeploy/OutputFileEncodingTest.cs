namespace Net.Sf.Dbdeploy
{
    using System;
    using System.Text;

    using NUnit.Framework;

    [TestFixture]
    public class OutputFileEncodingTest
    {
        [Test]
        public void ShouldKnowWhenValidEncoding()
        {
            Assert.IsTrue(new OutputFileEncoding("iso-8859-1").IsValid());
        }
        [Test]
        public void ShouldKnowWhenIsInvalidEncoding()
        {
            Assert.IsFalse(new OutputFileEncoding("invalid").IsValid());
        }

        [Test]
        [ExpectedException(typeof (ArgumentException))]
        public void WhenInvalidEncodingShouldThrowException()
        {
            new OutputFileEncoding("dfjad").AsEncoding();
        }

        [Test]
        public void WhenNotSpecifiedShouldReturnDefaultEncoding()
        {
            Assert.AreEqual(Encoding.Default, new OutputFileEncoding(string.Empty).AsEncoding());
            Assert.AreEqual(Encoding.Default, new OutputFileEncoding("").AsEncoding());
            Assert.AreEqual(Encoding.Default, new OutputFileEncoding(null).AsEncoding());
        }

        [Test]
        public void WhenValidEncodingProvidedShouldCreateEncoding()
        {
            //TODO: since the default is locale specific, need to find a better way to test the first one
            //Assert.AreEqual(Encoding.Default.BodyName, new OutputFileEncoding("iso-8859-1").AsEncoding().BodyName);
			Assert.AreEqual(Encoding.Default.BodyName, new OutputFileEncoding("koi8-r").AsEncoding().BodyName);
            Assert.AreEqual(Encoding.ASCII, new OutputFileEncoding("us-ascii").AsEncoding());
            Assert.AreEqual(Encoding.BigEndianUnicode, new OutputFileEncoding("unicodeFFFE").AsEncoding());
            Assert.AreEqual(Encoding.Unicode, new OutputFileEncoding("utf-16").AsEncoding());
            Assert.AreEqual(Encoding.UTF32, new OutputFileEncoding("utf-32").AsEncoding());
            Assert.AreEqual(Encoding.UTF7, new OutputFileEncoding("utf-7").AsEncoding());
            Assert.AreEqual(Encoding.UTF8, new OutputFileEncoding("utf-8").AsEncoding());
        }

        [Test]
        public void WhenValidEncodingWithLeadingOrTrailingSpaceShouldStillCreateEncoding()
        {
            Assert.AreEqual(Encoding.UTF8, new OutputFileEncoding(" utf-8 ").AsEncoding());
        }

    }
}

