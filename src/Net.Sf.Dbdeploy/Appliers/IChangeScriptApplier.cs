using System.Collections.Generic;
using Net.Sf.Dbdeploy.Scripts;

namespace Net.Sf.Dbdeploy.Appliers
{
    public interface IChangeScriptApplier
    {
        void Apply(IEnumerable<ChangeScript> changeScripts, bool createChangeLogTable);
    }
}