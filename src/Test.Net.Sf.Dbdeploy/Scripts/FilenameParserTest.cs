namespace Net.Sf.Dbdeploy.Scripts
{
    using Net.Sf.Dbdeploy.Exceptions;

    using NUnit.Framework;

    [TestFixture]
    public class FilenameParserTest
    {
        [Test]
        public void CanParseAnyFilenameThatStartsWithANumber()
        {
            FilenameParser parser = new FilenameParser();
            Assert.AreEqual(1, parser.ExtractScriptNumberFromFilename("0001_a_filename.txt"));
            Assert.AreEqual(1, parser.ExtractScriptNumberFromFilename("1_a_filename.txt"));
            Assert.AreEqual(1, parser.ExtractScriptNumberFromFilename("1 a filename.txt"));
            Assert.AreEqual(1, parser.ExtractScriptNumberFromFilename("1.txt"));
            Assert.AreEqual(123, parser.ExtractScriptNumberFromFilename("00123_something.txt"));
            Assert.AreEqual(1, parser.ExtractScriptNumberFromFilename("1.foo.2.txt"));
        }

        [Test, ExpectedException(typeof(UnrecognisedFilenameException), "Could not extract a change script number from filename: blah blah blah")]
        public void ThrowsWhenFilenameDoesNotStartWithANumber()
        {
            new FilenameParser().ExtractScriptNumberFromFilename("blah blah blah");
        }

        [Test, ExpectedException(typeof(UnrecognisedFilenameException), "Could not extract a change script number from filename: foo.2.txt")]
        public void ThrowsWhenFilenameDoesNotStartWithANumberAndContainsANumber()
        {
            new FilenameParser().ExtractScriptNumberFromFilename("foo.2.txt");
        }
    }
}