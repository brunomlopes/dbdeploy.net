using System;
using System.IO;

namespace Net.Sf.Dbdeploy.Database
{
    public class DbProviderFile
    {
        private string path;

        public DbProviderFile()
        {
            LoadFrom(Environment.CurrentDirectory + @"\dbproviders.xml");
        }

        public string Path
        {
            get { return path; }
        }

        public void LoadFrom(string providerPath)
        {
            path = providerPath;
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Could not load dbproviders.xml from " + path);
            }
        }
    }
}