namespace Net.Sf.Dbdeploy.Database
{
    using System;
    using System.Globalization;
    using System.Xml.Serialization;

    [XmlRoot("providers")]
    public class DbProviders
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
            foreach (DatabaseProvider provider in itemsField)
            {
                if (dbmsType.Equals(provider.Name))
                    return provider;
            }

            throw new NotImplementedException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "No provider for type '{0}' implemented." + Environment.NewLine + "Supported dbms: ora, mssql, mysql, firebird, postgre, sybase", 
                    dbmsType));
        }
    }
}