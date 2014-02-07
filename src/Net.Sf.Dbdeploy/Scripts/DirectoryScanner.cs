namespace Net.Sf.Dbdeploy.Scripts
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    using Net.Sf.Dbdeploy.Exceptions;

    public class DirectoryScanner : IScriptScanner
    {
        private readonly FilenameParser filenameParser;

        private readonly TextWriter infoTextWriter;

        private readonly Encoding encoding;
        private readonly DirectoryInfo directory;

        public DirectoryScanner(TextWriter infoTextWriter, Encoding encoding, DirectoryInfo directory)
        {
            this.filenameParser = new FilenameParser();
            
            this.infoTextWriter = infoTextWriter;
            this.encoding = encoding;
            this.directory = directory;
        }

        public List<ChangeScript> GetChangeScripts()
        {
            if (directory == null)
                return new List<ChangeScript>();

            try
            {
                this.infoTextWriter.WriteLine("Reading change scripts from directory '" + directory.FullName + "'...");
            }
            catch (IOException)
            {
                // ignore
            }

            List<ChangeScript> scripts = new List<ChangeScript>();

            foreach (FileInfo file in directory.GetFiles("*.*", SearchOption.AllDirectories))
            {
                if ((file.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                    continue;

                string filename = file.Name;

                try
                {
                    int scriptNumber = this.filenameParser.ExtractScriptNumberFromFilename(filename);

                    scripts.Add(new ChangeScript(file.Directory.Name, scriptNumber, file, this.encoding));
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