using System.Collections.Generic;
using Net.Sf.Dbdeploy.Database;

namespace Net.Sf.Dbdeploy.Scripts
{
    public interface IRepositorioScripts
    {
        IList<ChangeScript> ObterScriptsPendenteExecucao(UniqueChange lastChangeToApply);
        ICollection<ChangeScript> ObterTodosOsScripts();
        IList<ChangeEntry> ObterScriptsAplicados();
        IList<ChangeEntry> ObterScritpsExecutadosComErro();
        ChangeEntry ObterScriptExecutado(ChangeScript changeScript);
    }
}