using System;
using System.Data;
using System.IO;
using FluentAssertions;
using Net.Sf.Dbdeploy.Configuration;
using Net.Sf.Dbdeploy.Exceptions;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Database
{
    [TestFixture]
    public class DbmsFactoryTest
    {
        private readonly string SybaseDll = AppDomain.CurrentDomain.BaseDirectory + @"\Mocks\Fixtures\SybaseDllConnection\iAnywhere.Data.SQLAnywhere.v4.0.dll";
        private readonly string OracleDll = AppDomain.CurrentDomain.BaseDirectory + @"\Mocks\Fixtures\OracleDllConnection\Oracle.DataAccess.dll";
        private readonly string MySqlDll = AppDomain.CurrentDomain.BaseDirectory + @"\Mocks\Fixtures\MySqlDllConnection\MySql.Data.dll";
        private readonly string FirebirdDll = AppDomain.CurrentDomain.BaseDirectory + @"\Mocks\Fixtures\FirebirdDllConnection\FirebirdSql.Data.FirebirdClient.dll";
        private readonly string PostgreDll = AppDomain.CurrentDomain.BaseDirectory + @"\Mocks\Fixtures\PostgreDllConnection\Npgsql.dll";

        private const string connectionStringSybase = "UID=dbdeploy;PWD=dbdeploy;DBN=dbdeploy;host=localhost:2638";
        private const string connectionStringMsSql = @"Server=localhost\SQLEXPRESS;Database=dbdeploy;User Id=sa;Password=sa;";
        private const string connectionStringOracleTns = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=XE)));User Id=dbdeploy;Password=dbdeploy;";
        private const string connectionStringMySql = "Server=localhost; Database=dbdeploy; Uid=dbdeploy; Pwd=dbdeploy;";
        private readonly string connectionStringFirebird = string.Format("User=SYSDBA;Password=masterkey;Database={0};DataSource=localhost;Port=3050;", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mocks", "Fixtures", "DatabaseFirebird", "DBDEPLOY.FDB"));
        private const string connectionStringPostgre = "Server=localhost;Port=5432;Database=dbdeploy;User Id=postgres;Password=postgres;";

        private DbmsFactory dbmsFactory;

        #region SyBase
        [Test]
        public void criar_instancia_de_sybase()
        {
            dbmsFactory = new DbmsFactory(SupportedDbms.SYBASE, connectionStringSybase);
            var dbmsSyntax = dbmsFactory.CreateDbmsSyntax();

            dbmsSyntax.Should().NotBeNull();
            dbmsSyntax.Should().BeOfType<SybaseDbmsSyntax>();
        }

        [Test]
        public void efetuar_conexao_com_banco_sybase()
        {
            dbmsFactory = new DbmsFactory(SupportedDbms.SYBASE, connectionStringSybase, SybaseDll);

            var connection = OpenConnection(dbmsFactory);

            connection.Should().NotBeNull();
            connection.State.Should().Be(ConnectionState.Open);
        }

        [Test]
        public void executar_script_sql_sybase()
        {
            dbmsFactory = new DbmsFactory(SupportedDbms.SYBASE, connectionStringSybase, SybaseDll);
            var connection = OpenConnection(dbmsFactory);
            const string sql = "SELECT * FROM SYSOBJECTS where name = 'SYSTYPEMAP'";

            var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Invoking(x => x.ExecuteNonQuery()).ShouldNotThrow();
        }

        [Test]
        public void lancar_excecao_quando_nao_for_um_banco_conhecido()
        {
            this.Invoking(t => { dbmsFactory = new DbmsFactory("sgbdDesconhecido", "connectionString"); }).ShouldThrow<DbmsNotSupportedException>();
        }
        #endregion

        #region MsSql
        public void instanciar_factory_mssql()
        {
            dbmsFactory = new DbmsFactory(SupportedDbms.MSSQL, connectionStringMySql);

            var dbmsSyntax = dbmsFactory.CreateDbmsSyntax();

            dbmsSyntax.Should().NotBeNull();
            dbmsSyntax.Should().BeOfType<MsSqlDbmsSyntax>();
        }

        [Test]
        public void efetuar_conexao_com_banco_mssql()
        {
            dbmsFactory = new DbmsFactory(SupportedDbms.MSSQL, connectionStringMsSql);

            var connection = OpenConnection(dbmsFactory);

            connection.Should().NotBeNull();
            connection.State.Should().Be(ConnectionState.Open);
        }

        [Test]
        public void executar_script_sql_mssql()
        {
            dbmsFactory = new DbmsFactory(SupportedDbms.MSSQL, connectionStringMsSql);
            var connection = OpenConnection(dbmsFactory);
            var command = connection.CreateCommand();
            command.CommandText = "Select getDate()";

            command.Invoking(x => x.ExecuteNonQuery()).ShouldNotThrow();
        }
        #endregion

        #region Oracle
        [Test]
        public void criar_instancia_de_oracle()
        {
            dbmsFactory = new DbmsFactory(SupportedDbms.ORACLE, connectionStringOracleTns);

            var dbmsSyntax = dbmsFactory.CreateDbmsSyntax();

            dbmsSyntax.Should().NotBeNull();
            dbmsSyntax.Should().BeOfType<OracleDbmsSyntax>();
        }

        [Test]
        public void efetuar_conexao_com_banco_oracle()
        {
            dbmsFactory = new DbmsFactory(SupportedDbms.ORACLE, connectionStringOracleTns, OracleDll);

            var connection = OpenConnection(dbmsFactory);

            connection.Should().NotBeNull();
            connection.State.Should().Be(ConnectionState.Open);
        }

        [Test]
        public void executar_script_sql_oracle()
        {
            dbmsFactory = new DbmsFactory(SupportedDbms.ORACLE, connectionStringOracleTns, OracleDll);
            var connection = OpenConnection(dbmsFactory);
            var command = connection.CreateCommand();
            command.CommandText = "Select * from sysdate from dual";

            command.Invoking(x => x.ExecuteNonQuery()).ShouldNotThrow();
        }
        #endregion

        #region MySQL
        [Test]
        public void criar_instancia_de_mysql_syntax()
        {
            dbmsFactory = new DbmsFactory(SupportedDbms.MYSQL, "connectionString");
            var dbmsSyntax = dbmsFactory.CreateDbmsSyntax();

            dbmsSyntax.Should().NotBeNull();
            dbmsSyntax.Should().BeOfType<MySqlDbmsSyntax>();
        }

        [Test]
        public void efetuar_conexao_com_banco_mysql()
        {
            dbmsFactory = new DbmsFactory(SupportedDbms.MYSQL, connectionStringMySql, MySqlDll);
            var connection = OpenConnection(dbmsFactory);

            connection.Should().NotBeNull();
            connection.State.Should().Be(ConnectionState.Open);
        }

        [Test]
        public void executar_script_sql_mysql()
        {
            dbmsFactory = new DbmsFactory(SupportedDbms.MYSQL, connectionStringMySql, MySqlDll);
            var connection = OpenConnection(dbmsFactory);
            var command = connection.CreateCommand();
            command.CommandText = "Select CURRENT_TIMESTAMP From Dual;";

            command.Invoking(x => x.ExecuteNonQuery()).ShouldNotThrow();
        }
        #endregion

        #region Firebird
        [Test]
        public void criar_instancia_de_firebird_sintaxe()
        {
            dbmsFactory = new DbmsFactory(SupportedDbms.FIREBIRD, "connectionString");

            var dbmsSyntax = dbmsFactory.CreateDbmsSyntax();

            dbmsSyntax.Should().NotBeNull();
            dbmsSyntax.Should().BeOfType<FirebirdDbmsSyntax>();
        }

        [Test]
        public void efetuar_conexao_com_banco_firebird()
        {
            dbmsFactory = new DbmsFactory(SupportedDbms.FIREBIRD, connectionStringFirebird, FirebirdDll);
            var connection = OpenConnection(dbmsFactory);

            connection.Should().NotBeNull();
            connection.State.Should().Be(ConnectionState.Open);
        }

        [Test]
        public void executar_script_sql_firebird()
        {
            dbmsFactory = new DbmsFactory(SupportedDbms.FIREBIRD, connectionStringFirebird, FirebirdDll);
            var connection = OpenConnection(dbmsFactory);
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM rdb$relation_fields";

            command.Invoking(x => x.ExecuteNonQuery()).ShouldNotThrow();
        }
        #endregion

        #region PostGre
        [Test]
        public void criar_instancia_de_postgre_sintaxe()
        {
            dbmsFactory = new DbmsFactory(SupportedDbms.POSTGRE, "connectionString");

            var dbmsSyntax = dbmsFactory.CreateDbmsSyntax();

            dbmsSyntax.Should().NotBeNull();
            dbmsSyntax.Should().BeOfType<PostgreDbmsSyntax>();
        }

        [Test]
        public void efetuar_conexao_com_banco_postgre()
        {
            dbmsFactory = new DbmsFactory(SupportedDbms.POSTGRE, connectionStringPostgre, PostgreDll);
            var connection = OpenConnection(dbmsFactory);

            connection.Should().NotBeNull();
            connection.State.Should().Be(ConnectionState.Open);
        }

        [Test]
        public void executar_script_sql_postgre()
        {
            dbmsFactory = new DbmsFactory(SupportedDbms.POSTGRE, connectionStringPostgre, PostgreDll);
            var connection = OpenConnection(dbmsFactory);
            var command = connection.CreateCommand();
            command.CommandText = "SELECT CURRENT_DATE;";

            command.Invoking(x => x.ExecuteNonQuery()).ShouldNotThrow();
        }
        #endregion

        private IDbConnection OpenConnection(DbmsFactory factory)
        {
            var connection = factory.CreateConnection();
            connection.Open();
            return connection;
        }
    }
}
