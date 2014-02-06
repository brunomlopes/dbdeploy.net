using System.Reflection;

namespace Net.Sf.Dbdeploy.Scripts.EmbeddedResources
{
    public class EmbeddedFileInfo
    {
        public Assembly Assembly { get; set; }
        public string ResourceName { get; set; }
        public string FileName { get; set; }
        public string Folder { get; set; }
    }
}