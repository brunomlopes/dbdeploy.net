using System.Xml.Serialization;

namespace Net.Sf.Dbdeploy.Database
{
    public class DatabaseProvider
    {
        private string nameField;
        private string descriptionField;
        private string assemblyNameField;
        private string connectionClassField;

        /// <remarks/>
        [XmlAttribute("name")]
        public string Name
        {
            get { return nameField; }
            set { nameField = value; }
        }

        /// <remarks/>
        [XmlAttribute("description")]
        public string Description
        {
            get { return descriptionField; }
            set { descriptionField = value; }
        }

        /// <remarks/>
        [XmlAttribute("assemblyName")]
        public string AssemblyName
        {
            get { return assemblyNameField; }
            set { assemblyNameField = value; }
        }

        /// <remarks/>
        [XmlAttribute("connectionClass")]
        public string ConnectionClass
        {
            get { return connectionClassField; }
            set { connectionClassField = value; }
        }
    }
}