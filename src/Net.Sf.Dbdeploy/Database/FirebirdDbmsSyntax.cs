using System;
using System.Collections.Generic;
using System.Text;
using Net.Sf.Dbdeploy.Configuration;
using System.Collections;

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