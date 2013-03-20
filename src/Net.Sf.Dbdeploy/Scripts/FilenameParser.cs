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
            this.pattern = new Regex(@"^(\d+)", RegexOptions.Compiled);
        }

        public int ExtractScriptNumberFromFilename(string filename)
        {
            Match match = this.pattern.Match(filename);
            
            if (!match.Success || match.Groups.Count != 2)
                throw new UnrecognisedFilenameException("Could not extract a change script number from filename: " + filename);

            return int.Parse(match.Groups[1].Value);
        }
    }
}