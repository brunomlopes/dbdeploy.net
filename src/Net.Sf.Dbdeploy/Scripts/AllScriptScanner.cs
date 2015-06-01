using System.Collections.Generic;

namespace Net.Sf.Dbdeploy.Scripts
{
    public class AllScriptScanner : IScriptScanner
    {
        private readonly IScriptScanner[] scanners;

        public AllScriptScanner(params IScriptScanner[] scanners)
        {
            this.scanners = scanners;
        }

        public List<ChangeScript> GetChangeScripts()
        {
            if(scanners == null)
                return new List<ChangeScript>();

            var scripts = new List<ChangeScript>();
            foreach (var scanner in scanners)
            {
                var changeScripts = scanner.GetChangeScripts();
                changeScripts.Sort();
                scripts.AddRange(changeScripts);
            }

            return scripts;
        }
    }
}