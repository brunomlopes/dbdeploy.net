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
            path = null;
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
            Stream providerStream;
            if (Path == null) providerStream = GetType().Assembly.GetManifestResourceStream(GetType(), ProviderFilename);
            else if (!File.Exists(Path)) throw new FileNotFoundException("Could not load provider file from " + path);
            else providerStream = File.OpenRead(Path);

            using (providerStream)
            using (XmlReader reader = new XmlTextReader(providerStream))
            {
                var serializer = new XmlSerializer(typeof (Providers));

                return (Providers) serializer.Deserialize(reader);
            }
        }
    }
}