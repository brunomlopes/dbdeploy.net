using System.Collections.Generic;
using Net.Sf.Dbdeploy.Exceptions;

namespace Net.Sf.Dbdeploy.Scripts
{
    public class ChangeScriptRepository : IAvailableChangeScriptsProvider
    {
        private readonly List<ChangeScript> scripts;

        public ChangeScriptRepository(List<ChangeScript> scripts)
        {
            this.scripts = scripts;

            scripts.Sort();

            CheckForDuplicateIds(scripts);
        }

        private static void CheckForDuplicateIds(List<ChangeScript> scripts)
        {
            int lastId = -1;

            foreach (ChangeScript script in scripts)
            {
                if (script.ScriptNumber == lastId)
                {
                    throw new DuplicateChangeScriptException("There is more than one change script with number " + lastId);
                }

                lastId = script.ScriptNumber;
            }
        }

        public ICollection<ChangeScript> GetAvailableChangeScripts()
        {
            return new List<ChangeScript>(scripts.AsReadOnly());
        }
    }
}