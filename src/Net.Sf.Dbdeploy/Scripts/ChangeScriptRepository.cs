using System.Collections.Generic;
using Net.Sf.Dbdeploy.Exceptions;

namespace Net.Sf.Dbdeploy.Scripts
{
    public class ChangeScriptRepository
    {
        private readonly List<ChangeScript> scripts;

        public ChangeScriptRepository(List<ChangeScript> scripts)
        {
            this.scripts = scripts;

            scripts.Sort();

            CheckForDuplicateIds(scripts);
        }

        private void CheckForDuplicateIds(List<ChangeScript> scripts)
        {
            int lastId = -1;

            foreach (ChangeScript script in scripts)
            {
                if (script.GetId() == lastId)
                {
                    throw new DuplicateChangeScriptException("There is more than one change script with number " +
                                                             lastId);
                }

                lastId = script.GetId();
            }
        }

        public List<ChangeScript> GetOrderedListOfDoChangeScripts()
        {
            return new List<ChangeScript>(scripts.AsReadOnly());
        }

        public List<ChangeScript> GetOrderedListOfUndoChangeScripts()
        {
            scripts.Reverse();
            return new List<ChangeScript>(scripts.AsReadOnly());
        }
    }
}