namespace Net.Sf.Dbdeploy
{
    using System.Collections.Generic;

    using Net.Sf.Dbdeploy.Scripts;

    public interface IChangeScriptApplier
    {
        void Apply(IEnumerable<ChangeScript> changeScripts, bool createChangeLogTable);
    }
}