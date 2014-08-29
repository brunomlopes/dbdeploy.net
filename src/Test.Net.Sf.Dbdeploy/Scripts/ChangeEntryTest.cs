using FluentAssertions;
using Net.Sf.Dbdeploy.Database;
using NUnit.Framework;

namespace Net.Sf.Dbdeploy.Scripts
{
    [TestFixture]
    public class ChangeEntryTest
    {
        [TestCase(ScriptStatus.Success)]
        [TestCase(ScriptStatus.SucessRevisedUser)]
        public void retornar_que_script_foi_executado_com_sucesso(ScriptStatus scriptStatus)
        {
            var changeEntry = new ChangeEntry("Folder", 1) { Status = scriptStatus };

            changeEntry.ExecutedSuccessfully.Should().BeTrue();
        }

        public void retorar_que_o_script_nao_foi_executado_com_sucesso()
        {
            var changeEntry = new ChangeEntry("Folder", 1) { Status = ScriptStatus.Failure };

            changeEntry.ExecutedSuccessfully.Should().BeFalse();
        }
    }
}
