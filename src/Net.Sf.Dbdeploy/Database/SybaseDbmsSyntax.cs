using Net.Sf.Dbdeploy.Configuration;

namespace Net.Sf.Dbdeploy.Database
{
    public class SybaseDbmsSyntax : DbmsSyntax
    {
        public SybaseDbmsSyntax() : base(SupportedDbms.SYBASE)
        {}

        public override string CurrentTimestamp
        {
            get { return "getdate()"; }
        }

        public override string CurrentUser
        {
            get { return "user_name()"; }
        }

        protected override string GetQueryTableExists(TableInfo tableInfo)
        {
            return string.Format("SELECT NAME FROM SYSOBJECTS WHERE NAME = '{0}'", tableInfo.TableName);
        }
    }
}