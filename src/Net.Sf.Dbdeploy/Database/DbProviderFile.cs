using System;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace Net.Sf.Dbdeploy.Database
{
    public class DbProviderFile
    {
        public const string ProviderFilename = @"dbproviders.xml";
        private string path;

        public DbProviderFile()
        {
            path = GetDefaultPath();
        }

        public string Path
        {
            get { return path; }
            set { path = value; }
        }

        private static string GetDefaultPath()
        {
            DirectoryInfo assemblyDirectory = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
            string path = System.IO.Path.Combine(assemblyDirectory.FullName, ProviderFilename);
            if (!File.Exists(path))
            {
                path = System.IO.Path.Combine(Environment.CurrentDirectory, ProviderFilename);
            }
            return path;
        }

        public Providers LoadProviders()
        {
            if (!File.Exists(Path))
            {
                throw new FileNotFoundException("Could not load provider file from " + path);
            }
            XmlSerializer serializer = new XmlSerializer(typeof(Providers));
            using (XmlReader reader = new XmlTextReader(Path))
            {
                return (Providers)serializer.Deserialize(reader);
            }
        }
    }
}