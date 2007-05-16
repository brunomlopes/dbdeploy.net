using System;
using System.IO;

namespace Net.Sf.Dbdeploy.Database
{
    public class DbProviderFile
    {
        private const string ProviderFilename = @"dbproviders.xml";
        private string path;

        public DbProviderFile()
        {
            LoadFrom(System.IO.Path.Combine(Environment.CurrentDirectory, ProviderFilename));
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
                throw new FileNotFoundException("Could not load provider file from " + path);
            }
        }
    }
}