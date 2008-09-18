using System;
using System.Text;

namespace Net.Sf.Dbdeploy
{
    public class OutputFileEncoding
    {
        private readonly string encoding;
        
        public OutputFileEncoding(string encoding)
        {
            this.encoding = encoding;
        }

        public Encoding AsEncoding()
        {
            if (string.IsNullOrEmpty(encoding))
                return Encoding.Default;

            return GetEncoding();
        }

        public bool IsValid()
        {
            try
            {
                GetEncoding();
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        private Encoding GetEncoding()
        {
            return Encoding.GetEncoding(encoding.Trim());
        }
    }
}