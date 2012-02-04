using System;
using System.Collections.Generic;
using System.Text;

namespace Net.Sf.Dbdeploy.Database
{
    public class QueryStatementSplitter
    {
        private string delimiter = ";";

        private IDelimiterType delimiterType = new NormalDelimiter();

        private string lineEnding = Database.LineEnding.Platform;

        public QueryStatementSplitter()
        {
        }

        public string Delimiter
        {
            get { return this.delimiter; }
            set { this.delimiter = value; }
        }

        public IDelimiterType DelimiterType
        {
            get { return this.delimiterType; }
            set { this.delimiterType = value; }
        }

        public string LineEnding
        {
            get { return this.lineEnding; }
            set { this.lineEnding = value; }
        }

        public IEnumerable<string> Split(string input)
        {
            var statements = new List<string>();
            var currentSql = new StringBuilder();
            string[] lines = input.Split("\r\n".ToCharArray());

            foreach (string line in lines)
            {
                string strippedLine = line.TrimEnd();

                if (string.IsNullOrEmpty(strippedLine))
                    continue;

                if (currentSql.Length != 0)
                {
                    currentSql.Append(this.lineEnding);
                }

                currentSql.Append(strippedLine);

                if (this.delimiterType.Matches(strippedLine, this.delimiter))
                {
                    statements.Add(currentSql.ToString(0, currentSql.Length - this.delimiter.Length));

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