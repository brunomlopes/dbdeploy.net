using System.Collections.Generic;
using Net.Sf.Dbdeploy.Scripts;

namespace Net.Sf.Dbdeploy
{
    public interface IAvailableChangeScriptsProvider
    {
        ICollection<ChangeScript> GetAvailableChangeScripts();
    }
}
