using System.Collections.Generic;

namespace Net.Sf.Dbdeploy.Scripts
{
    public interface IScriptScanner
    {
        List<ChangeScript> GetChangeScripts();
    }
}