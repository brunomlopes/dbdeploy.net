namespace Net.Sf.Dbdeploy.Database
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Serialization;

    public class DbProviderFile
    {
        public const string ProviderFilename = @"dbproviders.xml";

        public DbProviderFile()
        {
            this.Path = null;
        }

        public string Path { get; set; }

        public DbProviders LoadProviders()
        {
            Stream providerStream;
            if (!string.IsNullOrWhiteSpace(Path) && !System.IO.File.Exists(Path))
                throw new FileNotFoundException("File not found for loading providers", Path);
            var path = ProviderFilename;
            if (!File.Exists(path))
                path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(GetType().Assembly.Location), path);

            if (File.Exists(path))
                providerStream = File.OpenRead(path);
            else
                providerStream = GetType().Assembly.GetManifestResourceStream(GetType(), ProviderFilename);

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