namespace Net.Sf.Dbdeploy.Scripts
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    using Net.Sf.Dbdeploy.Exceptions;

    public class DirectoryScanner
    {
        private readonly FilenameParser filenameParser;

        private readonly TextWriter infoTextWriter;

        private readonly Encoding encoding;

        public DirectoryScanner(TextWriter infoTextWriter, Encoding encoding)
        {
            this.filenameParser = new FilenameParser();
            
            this.infoTextWriter = infoTextWriter;
            this.encoding = encoding;
        }

        public List<ChangeScript> GetChangeScriptsForDirectory(DirectoryInfo directory)
        {
            if (directory == null)
                throw new ArgumentNullException("directory");

            try
            {
                this.infoTextWriter.WriteLine("Reading change scripts from directory " + directory.FullName + "...");
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