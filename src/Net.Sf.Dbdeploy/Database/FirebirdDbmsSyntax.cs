using System;
using System.Text;
using Net.Sf.Dbdeploy.Configuration;

namespace Net.Sf.Dbdeploy.Database
{
    public class FirebirdDbmsSyntax : DbmsSyntax
    {
        public FirebirdDbmsSyntax() : base("firebird")
        {}

        public override string CurrentTimestamp
        {
            get { return "CURRENT_TIMESTAMP"; }
        }

        protected override string GetQueryCreateGenerator()
        {
            var sql = new StringBuilder();
            sql.Append("CREATE GENERATOR GenId;");
            sql.Append("SET GENERATOR GenId TO 0;");
            sql.Append(@"CREATE TRIGGER Trigger_ChangeLog FOR ChangeLog
ACTIVE BEFORE INSERT POSITION 0
AS
BEGIN
if (NEW.ChangeId is NULL) then NEW.ChangeId = GEN_ID(GenId, 1);
END;");

            return sql.ToString();
        }

        public override string CurrentUser
        {
            get { return "CURRENT_USER"; }
        }

        protected override string GetQueryTableExists(TableInfo tableInfo)
        {
            return string.Format("SELECT * FROM rdb$relation_fields WHERE rdb$relation_name = '{0}'", tableInfo.TableName.ToUpper());
        }
    }
}