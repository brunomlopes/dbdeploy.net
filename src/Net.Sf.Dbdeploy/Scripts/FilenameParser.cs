namespace Net.Sf.Dbdeploy.Scripts
{
    using System.Text.RegularExpressions;

    using Exceptions;

    public class FilenameParser
    {
        private readonly Regex pattern;

        public FilenameParser()
        {
            pattern = new Regex(@"^(\d+)", RegexOptions.Compiled);
        }

        public int ExtractScriptNumberFromFilename(string filename)
        {
            var match = pattern.Match(filename);
            
            if (!match.Success || match.Groups.Count != 2)
                throw new UnrecognisedFilenameException("Could not extract a change script number from filename: " + filename);

            return int.Parse(match.Groups[1].Value);
        }
    }
}