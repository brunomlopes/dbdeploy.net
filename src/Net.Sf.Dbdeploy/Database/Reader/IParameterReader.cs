using System.Data;

namespace Net.Sf.Dbdeploy.Database.Reader
{
    public interface IParameterReader
    {
        /// <summary>
        /// Get string value
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="name"></param>
        /// <returns>string value</returns>
        string GetString(IDataReader reader, string name);

        /// <summary>
        /// Get short value
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="name"></param>
        /// <returns>short value</returns>
        short GetShort(IDataReader reader, string name);

        /// <summary>
        /// Get byte value
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="name"></param>
        /// <returns>byte value</returns>
        byte GetByte(IDataReader reader, string name);

        /// <summary>
        /// Get long text for a certains dbms
        /// For example: BLOB from Firebird or others
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="name"></param>
        /// <returns>byte value</returns>
        string GetLongValueString(IDataReader reader, string name);
    }
}