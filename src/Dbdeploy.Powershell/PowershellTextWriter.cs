using System;
using System.IO;
using System.Management.Automation;
using System.Text;

namespace Dbdeploy.Powershell.Commands
{
    public class PowershellTextWriter : TextWriter
    {
        public Cmdlet Cmdlet { get; private set; }
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

        public PowershellTextWriter(Cmdlet cmdlet)
        {
            Cmdlet = cmdlet;
        }


        public PowershellTextWriter(IFormatProvider formatProvider, Cmdlet cmdlet) : base(formatProvider)
        {
            Cmdlet = cmdlet;
        }

        public override void Write(string value)
        {
            Cmdlet.WriteObject(value);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            Write(new string(buffer, index, count));
        }
    }
}