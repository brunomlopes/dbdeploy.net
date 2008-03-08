using System;
using System.Text;

namespace Net.Sf.Dbdeploy.Database
{
    public class OracleSqlPlusDbmsSyntax : DbmsSyntax
    {
        public string GenerateScriptHeader()
        {
            StringBuilder builder = new StringBuilder();
            /* Halt the script on error. */
            builder.Append("WHENEVER SQLERROR EXIT sql.sqlcode ROLLBACK");
            builder.Append(Environment.NewLine);
            /* Disable '&' variable substitution. */
            builder.Append("SET DEFINE OFF");
            return builder.ToString();
        }

        public string GenerateTimestamp()
        {
            return "CURRENT_TIMESTAMP";
        }

        public string GenerateUser()
        {
            return "USER";
        }

        public string GenerateStatementDelimiter()
        {
            return ";";
        }

        public string GenerateCommit()
        {
            return "COMMIT" + GenerateStatementDelimiter();
        }

		public string GenerateBeginTransaction()
		{
			return string.Empty;
		}

		public string GenerateCommitTransaction()
		{
			return string.Empty;
		}
	}
}