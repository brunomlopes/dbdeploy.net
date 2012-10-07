using NVelocity.Runtime.Resource.Loader;

namespace Net.Sf.Dbdeploy.Appliers.NVelocityReplacements
{
    /// <summary>
    /// We'll be internalizing the NVelocity library, and we need a textual reference to the assembly resource loader type.
    /// So we just subclass it, and point always to this one
    /// </summary>
    public class DbDeployAssemblyResourceLoader : AssemblyResourceLoader
    {

    }
}