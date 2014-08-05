using FluentAssertions;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Database
{
    [TestFixture]
    public class SybaseDbmsSyntaxTest
    {
        private SybaseDbmsSyntax SybaseDbmsSyntax;

        [SetUp]
        public void SetUp()
        {
            SybaseDbmsSyntax = new SybaseDbmsSyntax();
        }

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
    }
}