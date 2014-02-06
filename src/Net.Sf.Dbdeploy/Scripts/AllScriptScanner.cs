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
                scripts.AddRange(scanner.GetChangeScripts());
            }

            return scripts;
        }
    }
}