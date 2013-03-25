namespace Dbdeploy.Powershell
{
    using System.Text;
    using System.Text.RegularExpressions;

    public class DescriptionPrettyPrinter
    {
        private readonly Regex numberRegex = new Regex("(?<number>[0-9]+)[-_ ](?<rest>.+)");
        private readonly Regex uppercaseRegex = new Regex("(?<l>[A-Z])");
        private readonly Regex wordSeparatorsRegex = new Regex("(?<l>[_-])");
        private readonly Regex manySpacesRegex = new Regex("(?<l>[ ][ ]+)");
        public string Format(string description)
        {
            var newDescription= new StringBuilder();

            var numberMatch = numberRegex.Match(description);
            if (numberMatch.Success)
            {
                newDescription.AppendFormat("{0} - ", numberMatch.Groups["number"].Value);
                description = numberMatch.Groups["rest"].Value;
                
            }
            description = uppercaseRegex.Replace(description, " ${l}");
            description = wordSeparatorsRegex.Replace(description, " ");
            description = manySpacesRegex.Replace(description, " ");
            newDescription.AppendFormat(description.Trim());
            
            return newDescription.ToString();
        }
    }
}