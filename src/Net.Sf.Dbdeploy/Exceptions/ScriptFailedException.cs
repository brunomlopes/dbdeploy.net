using System;
using System.Data.Common;

namespace Net.Sf.Dbdeploy.Exceptions
{
    public class ScriptFailedException : Exception
    {
        public DbException Cause { get; private set; }
        public string Script { get; private set; }
        public int Statement { get; private set; }
        public string ExecutedSql { get; private set; }

        public ScriptFailedException(DbException cause, string script, int statement, string executedSql)
        {
            Cause = cause;
            Script = script;
            Statement = statement;
            ExecutedSql = executedSql;
        }
    }
}