using System;
using System.Data;
using System.Linq;

namespace Net.Sf.Dbdeploy.Database.Reader
{
    public class FirebirdParameterReader : DefaultParameterReader
    {
        public override string GetLongValueString(IDataReader reader, string name)
        {
            // Handle DBNull values.
            var columnValue = reader[name];
            if (columnValue != DBNull.Value)
            {
                var valueByte = (byte[])columnValue;
                return valueByte.Aggregate("", (current, currentByte) => current + (char)currentByte);
            }
            return string.Empty;
        }
    }
}