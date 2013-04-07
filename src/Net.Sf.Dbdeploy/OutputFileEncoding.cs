namespace Net.Sf.Dbdeploy
{
    using System;
    using System.Text;

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

            return this.GetEncoding();
        }

        public bool IsValid()
        {
            try
            {
                this.GetEncoding();

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