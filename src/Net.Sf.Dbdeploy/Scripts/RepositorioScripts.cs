using System.Collections.Generic;
using System.Linq;
using Net.Sf.Dbdeploy.Database;

namespace Net.Sf.Dbdeploy.Scripts
{
    public class RepositorioScripts : IRepositorioScripts
    {
        private readonly IDatabaseSchemaVersionManager databaseSchemaVersionManager;
        private readonly IAvailableChangeScriptsProvider changeScriptRepository;
        
        public RepositorioScripts(IDatabaseSchemaVersionManager databaseSchemaVersionManager, IAvailableChangeScriptsProvider changeScriptRepository)
        {
            this.databaseSchemaVersionManager = databaseSchemaVersionManager;
            this.changeScriptRepository = changeScriptRepository;
        }

        public IList<ChangeScript> ObterScriptsPendenteExecucao(UniqueChange lastChangeToApply)
        {
            var scriptsAplicados = ObterScriptsAplicados();
            var todosScripts = ObterTodosOsScripts();
            return IdentificarScriptsQueFaltamExecutar(lastChangeToApply, todosScripts, scriptsAplicados);
        }

        public ICollection<ChangeScript> ObterTodosOsScripts()
        {
            return changeScriptRepository.GetAvailableChangeScripts();
        }

        public IList<ChangeEntry> ObterScriptsAplicados()
        {
            return databaseSchemaVersionManager.GetAppliedChanges();
        }

        public IList<ChangeEntry> ObterScritpsExecutadosComErro()
        {
            var scriptsAplicados = ObterScriptsAplicados();
            return scriptsAplicados.Where(x => x.Status == ScriptStatus.Failure).ToList();
        }

        public ChangeEntry ObterScriptExecutado(ChangeScript changeScript)
        {
            var scriptsAplicados = ObterScriptsAplicados();
            return scriptsAplicados.FirstOrDefault(x => x.CompareTo(changeScript) == 0);
        }

        private IList<ChangeScript> IdentificarScriptsQueFaltamExecutar(UniqueChange lastChangeToApply, IEnumerable<ChangeScript> scripts, IList<ChangeEntry> aplicados)
        {
            var listaScriptsParaAplicar = new List<ChangeScript>();

            // Re-run any scripts that have not been run, or are failed or resolved.
            // The check to exit on previous failure is done before this call.
            foreach (var script in scripts)
            {
                // If script has not been run yet, add it to the list.
                bool applyScript = false;
                var changeEntry = aplicados.FirstOrDefault(a => a.CompareTo(script) == 0);
                if (changeEntry == null)
                {
                    applyScript = true;
                }
                else
                {
                    // If the script has already been run check if it should be run again.
                    if (!changeEntry.ExecutedSuccessfully)
                    {
                        // Assign the ID so the record can be updated.
                        script.ChangeId = changeEntry.ChangeId;
                        applyScript = true;
                    }
                }

                if (applyScript)
                {
                    // Just add script if there is no cap specified.
                    if (lastChangeToApply == null)
                    {
                        listaScriptsParaAplicar.Add(script);
                    }
                    else if (script.CompareTo(lastChangeToApply) <= 0)
                    {
                        // Script is less than last change to apply.
                        listaScriptsParaAplicar.Add(script);
                    }
                    else
                    {
                        // Stop adding scripts as last change to apply has been met.
                        break;
                    }
                }
            }
            return listaScriptsParaAplicar;
        }
    }
}