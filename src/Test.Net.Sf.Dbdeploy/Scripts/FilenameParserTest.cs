using Net.Sf.Dbdeploy.Exceptions;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Scripts
{
    [TestFixture]
    public class FilenameParserTest
    {
        [Test]
        public void CanParseAnyFilenameThatStartsWithANumber()
        {
            FilenameParser parser = new FilenameParser();
            Assert.AreEqual(1, parser.ExtractIdFromFilename("0001_a_filename.txt"));
            Assert.AreEqual(1, parser.ExtractIdFromFilename("1_a_filename.txt"));
            Assert.AreEqual(1, parser.ExtractIdFromFilename("1 a filename.txt"));
            Assert.AreEqual(1, parser.ExtractIdFromFilename("1.txt"));
            Assert.AreEqual(123, parser.ExtractIdFromFilename("00123_something.txt"));
            Assert.AreEqual(1, parser.ExtractIdFromFilename("1.foo.2.txt"));
        }

        [Test, ExpectedException(typeof(UnrecognisedFilenameException), "Could not extract a change script number from filename: blah blah blah")]
        public void ThrowsWhenFilenameDoesNotStartWithANumber()
        {
            new FilenameParser().ExtractIdFromFilename("blah blah blah");
        }

        [Test, ExpectedException(typeof(UnrecognisedFilenameException), "Could not extract a change script number from filename: foo.2.txt")]
        public void ThrowsWhenFilenameDoesNotStartWithANumberAndContainsANumber()
        {
            new FilenameParser().ExtractIdFromFilename("foo.2.txt");
        }
    }
}