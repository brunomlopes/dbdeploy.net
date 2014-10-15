using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Net.Sf.Dbdeploy.Database
{
    /// <summary>
    /// Supports dollar quoted strings
    /// </summary>
    public class PostgresqlStatementSplitter : QueryStatementSplitter
    {
        private Regex DollarQuoteRegex = new Regex(@"(\$[a-zA-Z_]*\$)");

        public override ICollection<string> Split(string input)
        {
            // I am not proud of this code, but for now it works. 
            // There are some tests around it, so we can come back and fix it later.
            var statements = new List<string>();
            var currentSql = new StringBuilder();

            var lineEnumerator = input.Split("\r\n".ToCharArray())
                .Cast<string>()
                .GetEnumerator();

            while(lineEnumerator.MoveNext())
            {
                string strippedLine = lineEnumerator.Current.TrimEnd();

                if (string.IsNullOrEmpty(strippedLine))
                    continue;
                
                var match = DollarQuoteRegex.Match(strippedLine);
                
                if (match.Success)
                {
                    // if we get a dollar quoted string, just carry on until we find it again
                    var tag = match.Captures[0].Value;
                    var moved = true;
                    do
                    {
                        if(!string.IsNullOrEmpty(strippedLine))
                            currentSql.Append(LineEnding + strippedLine);
                    } while ((moved = lineEnumerator.MoveNext())
                             && !(strippedLine = lineEnumerator.Current.TrimEnd()).Contains(tag));

                    if(moved)
                        currentSql.Append(LineEnding + strippedLine);
                }
                else
                {
                    if (currentSql.Length != 0)
                    {
                        currentSql.Append(LineEnding);
                    }
                    currentSql.Append(strippedLine);
                }


                if (this.DelimiterType.Matches(strippedLine, this.Delimiter))
                {
                    statements.Add(currentSql.ToString(0, currentSql.Length - this.Delimiter.Length));

                    // Clear StringBuilder
                    currentSql.Length = 0;
                }
            }

            if (currentSql.Length != 0)
            {
                statements.Add(currentSql.ToString());
            }

            return statements;
        }
    }
}