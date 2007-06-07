using System;
using System.Text.RegularExpressions;
using Net.Sf.Dbdeploy.Exceptions;

namespace Net.Sf.Dbdeploy.Scripts
{
    public class FilenameParser
    {
        private readonly Regex pattern;

        public FilenameParser()
        {
            pattern = new Regex(@"^(\d+)");
        }

        public int ExtractIdFromFilename(String filename)
        {
            Match match = pattern.Match(filename);
            if (!match.Success || match.Groups.Count != 2)
            {
                throw new UnrecognisedFilenameException("Could not extract a change script number from filename: " + filename);
            }
            return Int32.Parse(match.Groups[1].Value);
        }
    }
}