using System;
using System.Data;

namespace Net.Sf.Dbdeploy.Database.Reader
{
    public class DefaultParameterReader : IParameterReader
    {
        public virtual string GetString(IDataReader reader, string name)
        {
            // Handle DBNull values.
            var columnValue = reader[name];
            if (columnValue != DBNull.Value)
            {
                return columnValue.ToString();
            }
            return string.Empty;
        }

        public virtual short GetShort(IDataReader reader, string name)
        {
            // Handle DBNull values.
            var columnValue = reader[name];
            if (columnValue != DBNull.Value)
            {
                return Convert.ToInt16(columnValue);
            }
            return 0;
        }

        public virtual byte GetByte(IDataReader reader, string name)
        {
            // Handle DBNull values.
            var columnValue = reader[name];
            if (columnValue != DBNull.Value)
            {
                return Convert.ToByte(columnValue);
            }
            return 0;
        }

        public virtual string GetLongValueString(IDataReader reader, string name)
        {
            return GetString(reader, name);
        }
    }
}