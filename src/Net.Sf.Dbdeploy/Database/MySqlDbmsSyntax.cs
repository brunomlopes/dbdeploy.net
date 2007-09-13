using System;
using System.Collections.Generic;
using System.Text;

namespace Net.Sf.Dbdeploy.Database
{
    public class MySqlDbmsSyntax : DbmsSyntax
    {
        public string GenerateScriptHeader()
        {
            return String.Empty;
        }

        public string GenerateTimestamp()
        {
            return "CURRENT_TIMESTAMP";
        }

        public string GenerateUser()
        {
            return "USER()";
        }

        public string GenerateStatementDelimiter()
        {
            return ";";
        }

        public string GenerateCommit()
        {
            return "COMMIT" + GenerateStatementDelimiter();
        }
    }
}
    