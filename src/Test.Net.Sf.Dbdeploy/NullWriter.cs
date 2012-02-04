using System.IO;
using System.Text;

namespace Net.Sf.Dbdeploy
{
    internal class NullWriter : TextWriter
    {
        public override Encoding Encoding
        {
            get { return Encoding.Default; }
        }
    }
}
