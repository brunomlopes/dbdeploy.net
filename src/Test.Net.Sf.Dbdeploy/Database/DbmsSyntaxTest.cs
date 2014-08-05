using FluentAssertions;
using Net.Sf.Dbdeploy.Configuration;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Database
{
    [TestFixture]
    public class DbmsSyntaxTest
    {
        //private SybaseDbmsSyntax SybaseDbmsSyntax;
        //private MsSqlDbmsSyntax MsSqlDbmsSyntax;
        //private OracleDbmsSyntax OracleDbmsSyntax;
        //private MySqlDbmsSyntax MySqlDbmsSyntax;
        //private FirebirdDbmsSyntax FirebirdDbmsSyntax;
        //private PostgreDbmsSyntax PostgreDbmsSyntax;

        private DbmsSyntax dbmsSyntax;

        //[SetUp]
        //public void SetUp()
        //{
        //    SybaseDbmsSyntax = new SybaseDbmsSyntax();
        //    MsSqlDbmsSyntax = new MsSqlDbmsSyntax();
        //    OracleDbmsSyntax = new OracleDbmsSyntax();
        //    MySqlDbmsSyntax = new MySqlDbmsSyntax();
        //    FirebirdDbmsSyntax = new FirebirdDbmsSyntax();
        //    PostgreDbmsSyntax = new PostgreDbmsSyntax();
        //}

        #region SyBase
        [Test]
        public void retornar_o_nome_do_sgbd()
        {
            Instanciar(BancosSuportados.SYBASE);
            dbmsSyntax.Dbms.Should().Be("sybase");
        }

        [Test]
        public void retornar_comando_data_e_hora_atual()
        {
            Instanciar(BancosSuportados.SYBASE);
            dbmsSyntax.CurrentTimestamp.Should().Be("getdate()");
        }

        [Test]
        public void retornar_comando_usuario_atual()
        {
            Instanciar(BancosSuportados.SYBASE);
            dbmsSyntax.CurrentUser.Should().Be("user_name()");
        }

        [Test]
        public void retornar_default_schema_em_branco()
        {
            Instanciar(BancosSuportados.SYBASE);
            dbmsSyntax.DefaultSchema.Should().BeEmpty();
        }

        [Test]
        public void retornar_query_que_verifica_se_existe_uma_tabela_no_banco()
        {
            Instanciar(BancosSuportados.SYBASE);
            const string queryQueDeveRetornar = "SELECT NAME FROM SYSOBJECTS WHERE NAME = 'ChangeLog'";

            var queryTableExists = dbmsSyntax.TableExists("ChangeLog");

            queryTableExists.Should().Be(queryQueDeveRetornar);
        }

        [Test]
        public void retornar_script_de_criacao_da_tabela_changelog_sybase()
        {
            Instanciar(BancosSuportados.SYBASE);
            const string SintaxeCreateTableChangeLog = "CREATE TABLE ChangeLog";
            var changeLogTableSqlScript = dbmsSyntax.CreateChangeLogTableSqlScript("ChangeLog");

            changeLogTableSqlScript.Should().Contain(SintaxeCreateTableChangeLog);
        }
        #endregion

        #region MsSql
        [Test]
        public void retornar_data_e_hora_atual_mssql()
        {
            Instanciar(BancosSuportados.MSSQL);
            dbmsSyntax.CurrentTimestamp.Should().Be("getdate()");
        }

        [Test]
        public void retornar_usuario_atual_mssql()
        {
            Instanciar(BancosSuportados.MSSQL);
            dbmsSyntax.CurrentUser.Should().Be("user_name()");
        }

        [Test]
        public void retornar_o_sgbd_mssql()
        {
            Instanciar(BancosSuportados.MSSQL);
            dbmsSyntax.Dbms.Should().Be("mssql");
        }

        [Test]
        public void retornar_default_schema_mssql()
        {
            Instanciar(BancosSuportados.MSSQL);
            dbmsSyntax.DefaultSchema.Should().Be("dbo");
        }

        [Test]
        public void retornar_informacoes_da_tabela_changelog_mssql()
        {
            Instanciar(BancosSuportados.MSSQL);
            var tableInfo = dbmsSyntax.GetTableInfo("ChangeLog");

            tableInfo.Should().NotBeNull();
            tableInfo.Schema.Should().Be("dbo");
            tableInfo.TableName.Should().Be("ChangeLog");
        }

        [Test]
        public void retornar_script_para_verificar_se_uma_tabela_existe_no_banco_mssql()
        {
            Instanciar(BancosSuportados.MSSQL);
            const string scriptEsperado = @"SELECT table_schema 
            FROM INFORMATION_SCHEMA.TABLES 
            WHERE TABLE_NAME = 'changelog' AND TABLE_SCHEMA = 'dbo'";

            var queryTableExists = dbmsSyntax.TableExists("ChangeLog");

            queryTableExists.Should().Contain(scriptEsperado);
        }
        #endregion

        #region Oracle
        [Test]
        public void retornar_data_e_hora_atual_oracle()
        {
            Instanciar(BancosSuportados.ORACLE);
            dbmsSyntax.CurrentTimestamp.Should().Be("CURRENT_TIMESTAMP");
        }

        [Test]
        public void retornar_usuario_atual_oracle()
        {
            Instanciar(BancosSuportados.ORACLE);
            dbmsSyntax.CurrentUser.Should().Be("USER");
        }

        [Test]
        public void retornar_sgbd_oracle()
        {
            Instanciar(BancosSuportados.ORACLE);
            dbmsSyntax.Dbms.Should().Be("ora");
        }

        [Test]
        public void retornar_query_para_verificar_se_tabela_existe()
        {
            Instanciar(BancosSuportados.ORACLE);
            const string queryEsperada = "SELECT * FROM USER_TABLES WHERE TABLE_NAME = 'CHANGELOG'";

            var queryTableExists = dbmsSyntax.TableExists("ChangeLog");

            queryTableExists.Should().Be(queryEsperada);
        }
        #endregion

        #region MySql
        [Test]
        public void retornar_data_e_hora_atual_mysql()
        {
            Instanciar(BancosSuportados.MYSQL);
            dbmsSyntax.CurrentTimestamp.Should().Be("CURRENT_TIMESTAMP");
        }

        [Test]
        public void retornar_usuario_atual_mysql()
        {
            Instanciar(BancosSuportados.MYSQL);
            dbmsSyntax.CurrentUser.Should().Be("USER()");
        }

        [Test]
        public void retornar_o_sgbd_mysql()
        {
            Instanciar(BancosSuportados.MYSQL);
            dbmsSyntax.Dbms.Should().Be("mysql");
        }

        [Test]
        public void montar_script_para_verificar_se_tabela_existe_passando_o_table_schema_mysql()
        {
            Instanciar(BancosSuportados.MYSQL);
            const string connectionString = "Server=localhost; Database=dbdeploy; Uid=dbdeploy; Pwd=dbdeploy;";
            const string scriptEsperado = @"SELECT table_schema 
            FROM INFORMATION_SCHEMA.TABLES 
            WHERE TABLE_NAME = 'ChangeLog' AND TABLE_SCHEMA = 'dbdeploy'";
            dbmsSyntax.SetDefaultDatabaseName(connectionString);

            var queryTableExists = dbmsSyntax.TableExists("ChangeLog");

            queryTableExists.Should().Be(scriptEsperado);
        }
        #endregion

        #region Firebird
        [Test]
        public void retornar_data_e_hora_atual_firebird()
        {
            Instanciar(BancosSuportados.FIREBIRD);
            dbmsSyntax.CurrentTimestamp.Should().Be("CURRENT_TIMESTAMP");
        }

        [Test]
        public void retornar_usuario_atual_firebird()
        {
            Instanciar(BancosSuportados.FIREBIRD);
            dbmsSyntax.CurrentUser.Should().Be("CURRENT_USER");
        }

        [Test]
        public void retornar_sgdb_firebird()
        {
            Instanciar(BancosSuportados.FIREBIRD);
            dbmsSyntax.Dbms.Should().Be("firebird");
        }

        [Test]
        public void retornar_query_para_verificar_se_tabela_existe_firebird()
        {
            Instanciar(BancosSuportados.FIREBIRD);
            const string scriptEsperado = "SELECT * FROM rdb$relation_fields WHERE rdb$relation_name = 'CHANGELOG'";

            var queryTableExists = dbmsSyntax.TableExists("ChangeLog");

            queryTableExists.Should().Be(scriptEsperado);
        }
        #endregion

        #region PostGre
        [Test]
        public void retornar_data_e_hora_atual_postgre()
        {
            Instanciar(BancosSuportados.POSTGRE);
            dbmsSyntax.CurrentTimestamp.Should().Be("CURRENT_DATE");
        }

        [Test]
        public void retornar_usuario_atual_postgre()
        {
            Instanciar(BancosSuportados.POSTGRE);
            dbmsSyntax.CurrentUser.Should().Be("CURRENT_USER");
        }

        [Test]
        public void retornar_sgbd_postgre()
        {
            Instanciar(BancosSuportados.POSTGRE);
            dbmsSyntax.Dbms.Should().Be("postgres");
        }

        [Test]
        public void retornar_query_para_verificar_se_tabela_existe_no_banco_postgre()
        {
            Instanciar(BancosSuportados.POSTGRE);
            const string scriptEsperado = @"SELECT table_schema 
            FROM INFORMATION_SCHEMA.TABLES 
            WHERE TABLE_NAME = 'changelog'";

            var queryTableExists = dbmsSyntax.TableExists("ChangeLog");

            queryTableExists.Should().Be(scriptEsperado);
        }
        #endregion

        private void Instanciar(string sgbd)
        {
            var dbmsFactory = new DbmsFactory(sgbd, "connection string");
            dbmsSyntax = (DbmsSyntax)dbmsFactory.CreateDbmsSyntax();
        }
    }
}