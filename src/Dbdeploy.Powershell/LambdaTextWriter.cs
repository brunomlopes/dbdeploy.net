namespace Dbdeploy.Powershell
{
    using System;
    using System.IO;
    using System.Text;

    public class LambdaTextWriter : TextWriter
    {
        private readonly Action<string> _writer;
        private UnicodeEncoding _encoding;

        public override Encoding Encoding
        {
            get
            {
                if (_encoding == null)
                {
                    _encoding = new UnicodeEncoding(false, false);
                }
                return _encoding;
            }
        }

        public LambdaTextWriter(Action<string> writer)
        {
            _writer = writer;
        }


        public LambdaTextWriter(IFormatProvider formatProvider, Action<string> writer)
            : base(formatProvider)
        {
            _writer = writer;
        }

        public override void Write(string value)
        {
            _writer(value);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            Write(new string(buffer, index, count));
        }
    }
}