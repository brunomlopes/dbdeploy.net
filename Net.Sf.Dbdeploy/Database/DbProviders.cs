using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Net.Sf.Dbdeploy.Database
{
    [XmlRoot("providers")]
    public class Providers
    {
        private DatabaseProvider[] itemsField;

        [XmlElement("provider")]
        public DatabaseProvider[] Items
        {
            get { return itemsField; }
            set { itemsField = value; }
        }

        public DatabaseProvider GetProvider(string dbmsType)
        {
            foreach(DatabaseProvider provider in itemsField)
            {
                if (dbmsType.Equals(provider.Name))
                    return provider;
            }
            return null;  // TODO: Add exception here for unknown type.
        }
    }

    
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