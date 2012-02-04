using System;
using System.Collections.Generic;
using System.IO;
using Net.Sf.Dbdeploy.Exceptions;

namespace Net.Sf.Dbdeploy.Scripts
{
    public class DirectoryScanner
    {
        private readonly FilenameParser filenameParser;

        private readonly TextWriter infoTextWriter;

        public DirectoryScanner(TextWriter infoTextWriter)
        {
            this.filenameParser = new FilenameParser();
            this.infoTextWriter = infoTextWriter;
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

                    scripts.Add(new ChangeScript(id, file));
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