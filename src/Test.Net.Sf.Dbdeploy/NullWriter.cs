namespace Net.Sf.Dbdeploy
{
    using System.IO;
    using System.Text;

    internal class NullWriter : TextWriter
    {
        public override Encoding Encoding
        {
            get { return Encoding.Default; }
        }
    }
}
