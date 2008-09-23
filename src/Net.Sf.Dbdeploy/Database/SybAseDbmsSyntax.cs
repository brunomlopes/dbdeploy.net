using System;

namespace Net.Sf.Dbdeploy.Database
{
    public class SybAseDbmsSyntax : DbmsSyntax
    {
        public override string GenerateScriptHeader()
        {
            return string.Empty;
        }

        public override string GenerateTimestamp()
        {
            return "getdate()";
        }

        public override string GenerateUser()
        {
            return "user_name()";
        }

        public override string GenerateStatementDelimiter()
        {
            return Environment.NewLine + "GO";
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
	}
}