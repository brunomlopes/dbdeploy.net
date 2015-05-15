namespace Net.Sf.Dbdeploy.Scripts
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    using Exceptions;

    public class DirectoryScanner : IScriptScanner
    {
        private readonly FilenameParser filenameParser;

        private readonly TextWriter infoTextWriter;

        private readonly Encoding encoding;
        private readonly DirectoryInfo directory;

        public DirectoryScanner(TextWriter infoTextWriter, Encoding encoding, DirectoryInfo directory)
        {
            filenameParser = new FilenameParser();
            
            this.infoTextWriter = infoTextWriter;
            this.encoding = encoding;
            this.directory = directory;
        }

        public List<ChangeScript> GetChangeScripts()
        {
            if (directory == null)
                return new List<ChangeScript>();
            if (!directory.Exists)
                return new List<ChangeScript>();

            try
            {
                infoTextWriter.WriteLine("Reading change scripts from directory '" + directory.FullName + "'...");
            }
            catch (IOException)
            {
                // ignore
            }

            var scripts = new List<ChangeScript>();

            foreach (FileInfo file in directory.GetFiles("*.sql", SearchOption.AllDirectories))
            {
                if ((file.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                    continue;

                string filename = file.Name;

                try
                {
                    var scriptNumber = filenameParser.ExtractScriptNumberFromFilename(filename);

                    scripts.Add(new ChangeScript(file.Directory.Name, scriptNumber, file, encoding));
                }
                catch (UnrecognisedFilenameException)
                {
                    // ignore
                }
            }

            return scripts;
        }
    }
}