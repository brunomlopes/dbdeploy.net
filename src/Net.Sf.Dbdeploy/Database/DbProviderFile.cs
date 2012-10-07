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
            this.path = null;
        }

        public string Path
        {
            get { return this.path; }
            set { this.path = value; }
        }

        public DbProviders LoadProviders()
        {
            Stream providerStream;
            if (Path == null) providerStream = GetType().Assembly.GetManifestResourceStream(GetType(), ProviderFilename);
            else if (!File.Exists(Path)) throw new FileNotFoundException("Could not load provider file from " + path);
            else providerStream = File.OpenRead(Path);

            using (providerStream)
            using (XmlReader reader = new XmlTextReader(providerStream))
            {
                var serializer = new XmlSerializer(typeof(DbProviders));

                return (DbProviders)serializer.Deserialize(reader);
            }
        }
        

        private static string GetDefaultPath()
        {
            DirectoryInfo assemblyDirectory = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;

            string providerFilePath = System.IO.Path.Combine(assemblyDirectory.FullName, ProviderFilename);
            
            if (!File.Exists(providerFilePath))
            {
                providerFilePath = System.IO.Path.Combine(Environment.CurrentDirectory, ProviderFilename);
            }
            
            return providerFilePath;
        }
    }
}
