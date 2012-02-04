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
            this.path = GetDefaultPath();
        }

        public string Path
        {
            get { return this.path; }
            set { this.path = value; }
        }

        public DbProviders LoadProviders()
        {
            if (!File.Exists(this.path))
                throw new FileNotFoundException("Could not load provider file from " + this.path);
            
            var serializer = new XmlSerializer(typeof(DbProviders));

            using (XmlReader reader = new XmlTextReader(this.path))
            {
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