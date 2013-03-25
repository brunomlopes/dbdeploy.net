namespace Net.Sf.Dbdeploy.Exceptions
{
    using System;
    using System.Data.Common;

    using Net.Sf.Dbdeploy.Scripts;

    public class ChangeScriptFailedException : DbDeployException
    {
        private readonly ChangeScript script;

        private readonly int statement;

        private readonly string executedSql;

        public ChangeScriptFailedException(DbException cause, ChangeScript script, int statement, string executedSql)
            : base(cause)
	    {
            this.script = script;
            this.statement = statement;
            this.executedSql = executedSql;
	    }

        public ChangeScript Script
        {
            get { return this.script;}
        }

        public string ExecutedSql
        {
            get { return this.executedSql;}
        }

        public int Statement
        {
            get { return this.statement;}
        }

        public override string  Message
        {
	        get
            {
                return "Change script " + this.script + " failed while executing statement " + this.statement + ":" + Environment.NewLine
                  + this.executedSql + Environment.NewLine 
                  + " -> " + this.InnerException.Message;
	        }
        }
    }
}

