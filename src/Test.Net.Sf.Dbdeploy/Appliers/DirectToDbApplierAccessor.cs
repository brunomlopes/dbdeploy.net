using System.IO;
using Net.Sf.Dbdeploy.Database;
using Net.Sf.Dbdeploy.Scripts;

namespace Net.Sf.Dbdeploy.Appliers
{
    internal class DirectToDbApplierAccessor : DirectToDbApplier
    {
        public DirectToDbApplierAccessor(
            QueryExecuter queryExecuter, 
            DatabaseSchemaVersionManager schemaVersionManager, 
            QueryStatementSplitter splitter, 
            TextWriter infoTextWriter) 
            : base(queryExecuter, schemaVersionManager, splitter, infoTextWriter)
        {
        }
        
        public new void ApplyChangeScript(ChangeScript script)
        {
            base.ApplyChangeScript(script);
        }

        public new void RecordScriptStatus(ChangeScript changeScript, string output)
        {
            base.RecordScriptStatus(changeScript, ScriptStatus.Success, output);
        }
    }
}
