using FluentAssertions;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Database
{
    [TestFixture]
    public class DbmsSyntaxTest
    {
        private SybaseDbmsSyntax SybaseDbmsSyntax;
        private MsSqlDbmsSyntax MsSqlDbmsSyntax;
        private OracleDbmsSyntax OracleDbmsSyntax;
        private MySqlDbmsSyntax MySqlDbmsSyntax;
        private FirebirdDbmsSyntax FirebirdDbmsSyntax;
        private PostgreDbmsSyntax PostgreDbmsSyntax;

        [SetUp]
        public void SetUp()
        {
            SybaseDbmsSyntax = new SybaseDbmsSyntax();
            MsSqlDbmsSyntax = new MsSqlDbmsSyntax();
            OracleDbmsSyntax = new OracleDbmsSyntax();
            MySqlDbmsSyntax = new MySqlDbmsSyntax();
            FirebirdDbmsSyntax = new FirebirdDbmsSyntax();
            PostgreDbmsSyntax = new PostgreDbmsSyntax();
        }

        #region SyBase
        [Test]
        public void retornar_o_nome_do_sgbd()
        {
            SybaseDbmsSyntax.Dbms.Should().Be("sybase");
        }

        [Test]
        public void retornar_comando_data_e_hora_atual()
        {
            SybaseDbmsSyntax.CurrentTimestamp.Should().Be("getdate()");
        }

        [Test]
        public void retornar_comando_usuario_atual()
        {
            SybaseDbmsSyntax.CurrentUser.Should().Be("user_name()");
        }

        [Test]
        public void retornar_default_schema_em_branco()
        {
            SybaseDbmsSyntax.DefaultSchema.Should().BeEmpty();
        }

        [Test]
        public void retornar_query_que_verifica_se_existe_uma_tabela_no_banco()
        {
            const string queryQueDeveRetornar = "SELECT NAME FROM SYSOBJECTS WHERE NAME = 'ChangeLog'";

            var queryTableExists = SybaseDbmsSyntax.TableExists("ChangeLog");

            queryTableExists.Should().Be(queryQueDeveRetornar);
        }

        [Test]
        public void retornar_script_de_criacao_da_tabela_changelog_sybase()
        {
            const string SintaxeCreateTableChangeLog = "CREATE TABLE ChangeLog";
            var changeLogTableSqlScript = SybaseDbmsSyntax.CreateChangeLogTableSqlScript("ChangeLog");

            changeLogTableSqlScript.Should().Contain(SintaxeCreateTableChangeLog);
        }
        #endregion

        #region MsSql
        [Test]
        public void retornar_data_e_hora_atual_mssql()
        {
            MsSqlDbmsSyntax.CurrentTimestamp.Should().Be("getdate()");
        }

        [Test]
        public void retornar_usuario_atual_mssql()
        {
            MsSqlDbmsSyntax.CurrentUser.Should().Be("user_name()");
        }

        [Test]
        public void retornar_o_sgbd_mssql()
        {
            MsSqlDbmsSyntax.Dbms.Should().Be("mssql");
        }

        [Test]
        public void retornar_default_schema_mssql()
        {
            MsSqlDbmsSyntax.DefaultSchema.Should().Be("dbo");
        }

        [Test]
        public void retornar_informacoes_da_tabela_changelog_mssql()
        {
            var tableInfo = MsSqlDbmsSyntax.GetTableInfo("ChangeLog");

            tableInfo.Should().NotBeNull();
            tableInfo.Schema.Should().Be("dbo");
            tableInfo.TableName.Should().Be("ChangeLog");
        }

        [Test]
        public void retornar_script_para_verificar_se_uma_tabela_existe_no_banco_mssql()
        {
            const string scriptEsperado = @"SELECT table_schema 
            FROM INFORMATION_SCHEMA.TABLES 
            WHERE TABLE_NAME = 'changelog' AND TABLE_SCHEMA = 'dbo'";

            var queryTableExists = MsSqlDbmsSyntax.TableExists("ChangeLog");

            queryTableExists.Should().Contain(scriptEsperado);
        }

        #endregion

        #region Oracle
        [Test]
        public void retornar_data_e_hora_atual_oracle()
        {
            OracleDbmsSyntax.CurrentTimestamp.Should().Be("CURRENT_TIMESTAMP");
        }

        [Test]
        public void retornar_usuario_atual_oracle()
        {
            OracleDbmsSyntax.CurrentUser.Should().Be("USER");
        }

        [Test]
        public void retornar_sgbd_oracle()
        {
            OracleDbmsSyntax.Dbms.Should().Be("ora");
        }

        [Test]
        public void retornar_query_para_verificar_se_tabela_existe()
        {
            const string queryEsperada = "SELECT * FROM USER_TABLES WHERE TABLE_NAME = 'CHANGELOG'";

            var queryTableExists = OracleDbmsSyntax.TableExists("ChangeLog");

            queryTableExists.Should().Be(queryEsperada);
        }
        #endregion

        #region MySql
        [Test]
        public void retornar_data_e_hora_atual_mysql()
        {
            MySqlDbmsSyntax.CurrentTimestamp.Should().Be("CURRENT_TIMESTAMP");
        }

        [Test]
        public void retornar_usuario_atual_mysql()
        {
            MySqlDbmsSyntax.CurrentUser.Should().Be("USER()");
        }

        [Test]
        public void retornar_o_sgbd_mysql()
        {
            MySqlDbmsSyntax.Dbms.Should().Be("mysql");
        }

        [Test]
        public void montar_script_para_verificar_se_tabela_existe_passando_o_table_schema_mysql()
        {
            const string connectionString = "Server=localhost; Database=dbdeploy; Uid=dbdeploy; Pwd=dbdeploy;";
            const string scriptEsperado = @"SELECT table_schema 
            FROM INFORMATION_SCHEMA.TABLES 
            WHERE TABLE_NAME = 'ChangeLog' AND TABLE_SCHEMA = 'dbdeploy'";
            MySqlDbmsSyntax.SetDefaultDatabaseName(connectionString);

            var queryTableExists = MySqlDbmsSyntax.TableExists("ChangeLog");

            queryTableExists.Should().Be(scriptEsperado);
        }
        #endregion

        #region Firebird
        [Test]
        public void retornar_data_e_hora_atual_firebird()
        {
            FirebirdDbmsSyntax.CurrentTimestamp.Should().Be("CURRENT_TIMESTAMP");
        }

        [Test]
        public void retornar_usuario_atual_firebird()
        {
            FirebirdDbmsSyntax.CurrentUser.Should().Be("CURRENT_USER");
        }

        [Test]
        public void retornar_sgdb_firebird()
        {
            FirebirdDbmsSyntax.Dbms.Should().Be("firebird");
        }

        [Test]
        public void retornar_query_para_verificar_se_tabela_existe_firebird()
        {
            const string scriptEsperado = "SELECT * FROM rdb$relation_fields WHERE rdb$relation_name = 'CHANGELOG'";

            var queryTableExists = FirebirdDbmsSyntax.TableExists("ChangeLog");

            queryTableExists.Should().Be(scriptEsperado);
        }
        #endregion

        #region PostGre
        [Test]
        public void retornar_data_e_hora_atual_postgre()
        {
            PostgreDbmsSyntax.CurrentTimestamp.Should().Be("CURRENT_DATE");
        }

        [Test]
        public void retornar_usuario_atual_postgre()
        {
            PostgreDbmsSyntax.CurrentUser.Should().Be("CURRENT_USER");
        }

        [Test]
        public void retornar_sgbd_postgre()
        {
            PostgreDbmsSyntax.Dbms.Should().Be("postgres");
        }

        [Test]
        public void retornar_query_para_verificar_se_tabela_existe_no_banco_postgre()
        {
            const string scriptEsperado = @"SELECT table_schema 
            FROM INFORMATION_SCHEMA.TABLES 
            WHERE TABLE_NAME = 'changelog'";

            var queryTableExists = PostgreDbmsSyntax.TableExists("ChangeLog");

            queryTableExists.Should().Be(scriptEsperado);
        }
        #endregion
    }
}