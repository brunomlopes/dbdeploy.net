using System;
using System.Data;
using FluentAssertions;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Database
{
    [TestFixture]
    public class DbmsFactorySybaseTest
    {
        private readonly string CaminhoDllConexao = AppDomain.CurrentDomain.BaseDirectory + @"\Mocks\Fixtures\SybaseDllConnection\iAnywhere.Data.SQLAnywhere.v4.0.dll";
        private readonly string ConnectionString = string.Format(@"UID=dbdeploy;PWD=dbdeploy;DBN=dbdeploy;DBF={0}", AppDomain.CurrentDomain.BaseDirectory + @"\Mocks\Fixtures\DatabaseSybase\dbdeploy.db");
        private const string NomeSgbd = "sybase";

        [Test]
        public void retornar_instancia_de_sybase()
        {
            var dbmsFactory = new DbmsFactory(NomeSgbd, ConnectionString, CaminhoDllConexao);
            var dbmsSyntax = dbmsFactory.CreateDbmsSyntax();

            dbmsSyntax.Should().NotBeNull();
            dbmsSyntax.Should().BeOfType<SybaseDbmsSyntax>();
        }

        [Test]
        public void abrir_a_conexao_com_o_banco()
        {
            var dbmsFactory = new DbmsFactory(NomeSgbd, ConnectionString, CaminhoDllConexao);

            var connection = OpenConnection(dbmsFactory);

            connection.Should().NotBeNull();
            connection.State.Should().Be(ConnectionState.Open);
        }

        [Test]
        public void executar_um_comando_sql()
        {
            var dbmsFactory = new DbmsFactory(NomeSgbd, ConnectionString, CaminhoDllConexao);
            var connection = OpenConnection(dbmsFactory);
            const string sql = "SELECT * FROM SYSOBJECTS where name = 'SYSTYPEMAP'";

            var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Invoking(x => x.ExecuteNonQuery()).ShouldNotThrow();
        }

        private IDbConnection OpenConnection(DbmsFactory factory)
        {
            var connection = factory.CreateConnection();
            connection.Open();
            return connection;
        }
    }
}
