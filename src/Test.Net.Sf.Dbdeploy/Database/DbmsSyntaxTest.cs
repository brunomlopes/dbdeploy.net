using FluentAssertions;
using Net.Sf.Dbdeploy.Configuration;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Database
{
    [TestFixture]
    public class DbmsSyntaxTest
    {
        private DbmsSyntax dbmsSyntax;

        #region SyBase
        [Test]
        public void retornar_o_nome_do_sgbd_sybase()
        {
            Instanciar(SupportedDbms.SYBASE);
            dbmsSyntax.Dbms.Should().Be(SupportedDbms.SYBASE);
        }

        [Test]
        public void retornar_comando_data_e_hora_atual_sybase()
        {
            Instanciar(SupportedDbms.SYBASE);
            dbmsSyntax.CurrentTimestamp.Should().Be("getdate()");
        }

        [Test]
        public void retornar_comando_usuario_atual_sybase()
        {
            Instanciar(SupportedDbms.SYBASE);
            dbmsSyntax.CurrentUser.Should().Be("user_name()");
        }

        [Test]
        public void retornar_default_schema_em_branco()
        {
            Instanciar(SupportedDbms.SYBASE);
            dbmsSyntax.DefaultSchema.Should().BeEmpty();
        }

        [Test]
        public void retornar_query_para_verificar_se_tabela_existe_sybase()
        {
            Instanciar(SupportedDbms.SYBASE);
            const string queryQueDeveRetornar = "SELECT NAME FROM SYSOBJECTS WHERE NAME = 'ChangeLog'";

            var queryTableExists = dbmsSyntax.TableExists("ChangeLog");

            queryTableExists.Should().Be(queryQueDeveRetornar);
        }

        [Test]
        public void retornar_query_para_verificar_se_tabela_sybase()
        {
            Instanciar(SupportedDbms.SYBASE);
            const string SintaxeCreateTableChangeLog = "CREATE TABLE ChangeLog";
            var changeLogTableSqlScript = dbmsSyntax.CreateChangeLogTableSqlScript("ChangeLog");

            changeLogTableSqlScript.Should().Contain(SintaxeCreateTableChangeLog);
        }
        #endregion

        #region MsSql
        [Test]
        public void retornar_comando_data_e_hora_atual_mssql()
        {
            Instanciar(SupportedDbms.MSSQL);
            dbmsSyntax.CurrentTimestamp.Should().Be("getdate()");
        }

        [Test]
        public void retornar_comando_usuario_atual_mssql()
        {
            Instanciar(SupportedDbms.MSSQL);
            dbmsSyntax.CurrentUser.Should().Be("user_name()");
        }

        [Test]
        public void retornar_o_nome_do_sgbd_mssql()
        {
            Instanciar(SupportedDbms.MSSQL);
            dbmsSyntax.Dbms.Should().Be(SupportedDbms.MSSQL);
        }

        [Test]
        public void retornar_default_schema_mssql()
        {
            Instanciar(SupportedDbms.MSSQL);
            dbmsSyntax.DefaultSchema.Should().BeEmpty();
        }

        [Test]
        public void retornar_informacoes_da_tabela_changelog()
        {
            Instanciar(SupportedDbms.MSSQL);
            var tableInfo = dbmsSyntax.GetTableInfo("ChangeLog");

            tableInfo.Should().NotBeNull();
            tableInfo.Schema.Should().BeEmpty();
            tableInfo.TableName.Should().Be("ChangeLog");
        }

        [Test]
        public void retornar_query_para_verificar_se_tabela_existe_mssql()
        {
            Instanciar(SupportedDbms.MSSQL);
            const string scriptEsperado = @"select object_id(concat(schema_name(), '.', 'ChangeLog'))";

            var queryTableExists = dbmsSyntax.TableExists("ChangeLog");

            queryTableExists.Should().Contain(scriptEsperado);
        }
        #endregion

        #region Oracle
        [Test]
        public void retornar_comando_data_e_hora_atual_oracle()
        {
            Instanciar(SupportedDbms.ORACLE);
            dbmsSyntax.CurrentTimestamp.Should().Be("CURRENT_TIMESTAMP");
        }

        [Test]
        public void retornar_comando_usuario_atual_oracle()
        {
            Instanciar(SupportedDbms.ORACLE);
            dbmsSyntax.CurrentUser.Should().Be("USER");
        }

        [Test]
        public void retornar_o_nome_do_sgbd_oracle()
        {
            Instanciar(SupportedDbms.ORACLE);
            dbmsSyntax.Dbms.Should().Be(SupportedDbms.ORACLE);
        }

        [Test]
        public void retornar_query_para_verificar_se_tabela_existe_oracle()
        {
            Instanciar(SupportedDbms.ORACLE);
            const string queryEsperada = "SELECT * FROM USER_TABLES WHERE TABLE_NAME = 'CHANGELOG'";

            var queryTableExists = dbmsSyntax.TableExists("ChangeLog");

            queryTableExists.Should().Be(queryEsperada);
        }
        #endregion

        #region MySql
        [Test]
        public void retornar_comando_data_e_hora_atual_mysql()
        {
            Instanciar(SupportedDbms.MYSQL);
            dbmsSyntax.CurrentTimestamp.Should().Be("CURRENT_TIMESTAMP");
        }

        [Test]
        public void retornar_comando_usuario_atual_mysql()
        {
            Instanciar(SupportedDbms.MYSQL);
            dbmsSyntax.CurrentUser.Should().Be("USER()");
        }

        [Test]
        public void retornar_o_nome_do_sgbd_mysql()
        {
            Instanciar(SupportedDbms.MYSQL);
            dbmsSyntax.Dbms.Should().Be(SupportedDbms.MYSQL);
        }

        [Test]
        public void retornar_query_para_verificar_se_tabela_existe_mysql()
        {
            Instanciar(SupportedDbms.MYSQL);
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
        public void retornar_comando_data_e_hora_atual_firebird()
        {
            Instanciar(SupportedDbms.FIREBIRD);
            dbmsSyntax.CurrentTimestamp.Should().Be("CURRENT_TIMESTAMP");
        }

        [Test]
        public void retornar_comando_usuario_atual_firebird()
        {
            Instanciar(SupportedDbms.FIREBIRD);
            dbmsSyntax.CurrentUser.Should().Be("CURRENT_USER");
        }

        [Test]
        public void retornar_o_nome_do_sgdb_firebird()
        {
            Instanciar(SupportedDbms.FIREBIRD);
            dbmsSyntax.Dbms.Should().Be(SupportedDbms.FIREBIRD);
        }

        [Test]
        public void retornar_query_para_verificar_se_tabela_existe_firebird()
        {
            Instanciar(SupportedDbms.FIREBIRD);
            const string scriptEsperado = "SELECT * FROM rdb$relation_fields WHERE rdb$relation_name = 'CHANGELOG'";

            var queryTableExists = dbmsSyntax.TableExists("ChangeLog");

            queryTableExists.Should().Be(scriptEsperado);
        }
        #endregion

        #region PostGre
        [Test]
        public void retornar_comando_data_e_hora_atual_postgre()
        {
            Instanciar(SupportedDbms.POSTGRE);
            dbmsSyntax.CurrentTimestamp.Should().Be("CURRENT_DATE");
        }

        [Test]
        public void retornar_comando_usuario_atual_postgre()
        {
            Instanciar(SupportedDbms.POSTGRE);
            dbmsSyntax.CurrentUser.Should().Be("CURRENT_USER");
        }

        [Test]
        public void retornar_o_nome_do_sgbd_postgre()
        {
            Instanciar(SupportedDbms.POSTGRE);
            dbmsSyntax.Dbms.Should().Be(SupportedDbms.POSTGRE);
        }

        [Test]
        public void retornar_query_para_verificar_se_tabela_existe_postgre()
        {
            Instanciar(SupportedDbms.POSTGRE);
            const string scriptEsperado = @"SELECT table_schema 
            FROM INFORMATION_SCHEMA.TABLES 
            WHERE TABLE_NAME = 'changelog'";

            var queryTableExists = dbmsSyntax.TableExists("ChangeLog");

            queryTableExists.Should().Be(scriptEsperado);
        }
        #endregion

        private void Instanciar(string sgbd)
        {
            var dbmsFactory = new DbmsFactory(sgbd, "connection=string");
            dbmsSyntax = (DbmsSyntax)dbmsFactory.CreateDbmsSyntax();
        }
    }
}