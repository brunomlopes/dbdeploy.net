using System.IO;
using Net.Sf.Dbdeploy.Appliers;
using Net.Sf.Dbdeploy.Configuration;
using Net.Sf.Dbdeploy.Database;
using Net.Sf.Dbdeploy.Scripts;

namespace Net.Sf.Dbdeploy
{
    /// <summary>
    /// Porta de entrada no dbdeploy para executar scripts individuais
    /// </summary>
    public class ExecutorScriptsIndividuais
    {
        private readonly DbDeployConfig dbDeployConfig;
        private readonly TextWriter textWriter;

        private DbmsFactory dbmsFactory;

        public ExecutorScriptsIndividuais(DbDeployConfig dbDeployConfig, TextWriter textWriter)
        {
            this.dbDeployConfig = dbDeployConfig;
            this.textWriter = textWriter;
        }

        private DbmsFactory DbmsFactory
        {
            get { return dbmsFactory ?? (dbmsFactory = new DbmsFactory(dbDeployConfig.Dbms, dbDeployConfig.ConnectionString, dbDeployConfig.DllPathConnector)); }
        }

        private IDbmsSyntax dbmsSyntax;
        private IDbmsSyntax DbmsSyntax
        {
            get { return dbmsSyntax ?? (dbmsSyntax = DbmsFactory.CreateDbmsSyntax()); }

        }

        private QueryExecuter queryExecuter;
        private QueryExecuter QueryExecuter
        {
            get { return queryExecuter ?? (queryExecuter = new QueryExecuter(DbmsFactory)); }
        }

        private DatabaseSchemaVersionManager databaseSchemaVersionManager;
        private DatabaseSchemaVersionManager DatabaseSchemaVersionManager
        {
            get { return databaseSchemaVersionManager ?? (databaseSchemaVersionManager = new DatabaseSchemaVersionManager(new QueryExecuter(DbmsFactory), DbmsSyntax, dbDeployConfig.ChangeLogTableName)); }
        }

        private DirectToDbApplier directToDbApplier;
        private DirectToDbApplier DirectToDbApplier
        {
            get { return directToDbApplier ?? (directToDbApplier = new DirectToDbApplier(QueryExecuter, DatabaseSchemaVersionManager, new QueryStatementSplitter(), DbmsSyntax, dbDeployConfig.ChangeLogTableName, textWriter)); }
        }

        /// <summary>
        /// Executa um determinado script passando um objeto ChangeScript
        /// </summary>
        /// <param name="changeScript"></param>
        public void Executar(ChangeScript changeScript)
        {
            QueryExecuter.Open();
            var criarChangeLog = VerificarSeDeveCriarTabelaChangeLog();
            DirectToDbApplier.ApplyChangeScript(changeScript, criarChangeLog);
            QueryExecuter.Close();
        }

        /// <summary>
        /// Executa um determinado script passando um objeto ChangeScript e seu respectivo conteúdo SQL
        /// </summary>
        /// <param name="changeScript"></param>
        /// <param name="scriptContent"></param>
        public void Executar(ChangeScript changeScript, string scriptContent)
        {
            QueryExecuter.Open();
            var criarChangeLog = VerificarSeDeveCriarTabelaChangeLog();
            DirectToDbApplier.ApplyScriptContent(changeScript, scriptContent, criarChangeLog);
            QueryExecuter.Close();
        }

        private bool VerificarSeDeveCriarTabelaChangeLog()
        {
            return dbDeployConfig.AutoCreateChangeLogTable && !DatabaseSchemaVersionManager.ChangeLogTableExists();
        }
    }
}