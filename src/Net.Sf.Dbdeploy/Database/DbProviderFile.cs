namespace Net.Sf.Dbdeploy.Database
{
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;

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
    }
}
