using System.Collections.Generic;

namespace Net.Sf.Dbdeploy
{
    public interface IAppliedChangesProvider
    {
        ICollection<int> GetAppliedChanges();
    }
}
