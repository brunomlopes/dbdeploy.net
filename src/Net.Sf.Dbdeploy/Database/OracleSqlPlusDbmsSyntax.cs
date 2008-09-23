using System;
using System.Text;

namespace Net.Sf.Dbdeploy.Database
{
    public class OracleSqlPlusDbmsSyntax : DbmsSyntax
    {
        public override string GenerateScriptHeader()
        {
            StringBuilder builder = new StringBuilder();
            /* Halt the script on error. */
            builder.Append("WHENEVER SQLERROR EXIT sql.sqlcode ROLLBACK");
            builder.Append(Environment.NewLine);
            /* Disable '&' variable substitution. */
            builder.Append("SET DEFINE OFF");
            return builder.ToString();
        }

        public override string GenerateTimestamp()
        {
            return "CURRENT_TIMESTAMP";
        }

        public override string GenerateUser()
        {
            return "USER";
        }

        public override string GenerateStatementDelimiter()
        {
            return ";";
        }

        public override string GenerateCommit()
        {
            return "COMMIT" + GenerateStatementDelimiter();
        }

		public override string GenerateBeginTransaction()
		{
			return string.Empty;
		}

		public override string GenerateCommitTransaction()
		{
			return string.Empty;
		}

		public override string GenerateVersionCheck(string tableName, string currentVersion, string deltaSet)
		{
			return String.Format("execute versionCheck('{0}', {1}, '{2}');", deltaSet, currentVersion, tableName);
		}
	}
}