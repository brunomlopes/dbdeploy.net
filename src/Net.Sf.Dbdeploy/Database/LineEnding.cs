using System;

namespace Net.Sf.Dbdeploy.Database
{
    public static class LineEnding
    {
        public static string Platform
        {
            get { return Environment.NewLine; }
        }

        public static string Cr
        {
            get { return "\r"; }
        }

        public static string CrLf
        {
            get { return "\r\n"; }
        }

        public static string Lf
        {
            get { return "\n"; }
        }
    }
}
