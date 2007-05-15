using Net.Sf.Dbdeploy.Exceptions;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Scripts
{
    [TestFixture]
    public class FilenameParserTest
    {
        [Test]
        public void TestCanParseAnyFilenameThatStartsWithANumber()
        {
            FilenameParser parser = new FilenameParser();
            Assert.AreEqual(1, parser.ExtractIdFromFilename("0001_a_filename.txt"));
            Assert.AreEqual(1, parser.ExtractIdFromFilename("1_a_filename.txt"));
            Assert.AreEqual(1, parser.ExtractIdFromFilename("1 a filename.txt"));
            Assert.AreEqual(1, parser.ExtractIdFromFilename("1.txt"));
            Assert.AreEqual(123, parser.ExtractIdFromFilename("00123_something.txt"));
        }

        [Test]
        public void TestThrowsWhenFilenameDoesNotStartWithANumber()
        {
            FilenameParser parser = new FilenameParser();
            try
            {
                parser.ExtractIdFromFilename("blah blah blah");
                Assert.Fail("expected exception");
            }
            catch (UnrecognisedFilenameException e)
            {
                Assert.AreEqual("Could not extract a change script number from filename: blah blah blah", e.Message);
            }
        }
    }
}