using System;
using System.Collections.Generic;
using System.IO;
using Net.Sf.Dbdeploy.Exceptions;
using System.Text;

namespace Net.Sf.Dbdeploy.Scripts
{
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

            foreach (FileInfo file in directory.GetFiles())
            {
                if ((file.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                    continue;

                string filename = file.Name;

                try
                {
                    int id = this.filenameParser.ExtractIdFromFilename(filename);

                    scripts.Add(new ChangeScript(id, file, this.encoding));
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