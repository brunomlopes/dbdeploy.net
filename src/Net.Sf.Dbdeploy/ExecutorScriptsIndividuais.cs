using System.IO;
using Net.Sf.Dbdeploy.Appliers;
using Net.Sf.Dbdeploy.Configuration;
using Net.Sf.Dbdeploy.Database;
using Net.Sf.Dbdeploy.Scripts;

namespace Net.Sf.Dbdeploy
{
    public class ExecutorScriptsIndividuais
    {
        private readonly DbDeployConfig dbDeployConfig;
        private readonly TextWriter textWriter;

        public ExecutorScriptsIndividuais(DbDeployConfig dbDeployConfig, TextWriter textWriter)
        {
            this.dbDeployConfig = dbDeployConfig;
            this.textWriter = textWriter;
        }

        public void Executar(ChangeScript changeScript)
        {
            var dbmsFactory = new DbmsFactory(dbDeployConfig.Dbms, dbDeployConfig.ConnectionString, dbDeployConfig.DllPathConnector);
            var dbmsSyntax = dbmsFactory.CreateDbmsSyntax();
            dbmsSyntax.SetDefaultDatabaseName(dbDeployConfig.ConnectionString);
            var queryExecuter = new QueryExecuter(dbmsFactory);
            var queryStatementSplitter = new QueryStatementSplitter();
            var databaseSchemaVersionManager = new DatabaseSchemaVersionManager(queryExecuter, dbmsSyntax, dbDeployConfig.ChangeLogTableName);
            var directToDbApplier = new DirectToDbApplier(queryExecuter, databaseSchemaVersionManager, queryStatementSplitter, dbmsSyntax, dbDeployConfig.ChangeLogTableName, textWriter);

            var criarChangeLog = VerificarSeDeveCriarTabelaChangeLog(databaseSchemaVersionManager);
            
            directToDbApplier.ApplyChangeScript(changeScript, criarChangeLog);

            queryExecuter.Close();
        }

        private bool VerificarSeDeveCriarTabelaChangeLog(IDatabaseSchemaVersionManager databaseSchemaVersionManager)
        {
            return dbDeployConfig.AutoCreateChangeLogTable && !databaseSchemaVersionManager.ChangeLogTableExists();
        }
    }
}