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
            filenameParser = new FilenameParser();
            this.infoTextWriter = infoTextWriter;
        }

        public List<ChangeScript> GetChangeScriptsForDirectory(DirectoryInfo directory)
        {
            try
            {
                infoTextWriter.WriteLine("Reading change scripts from directory " + directory.FullName + "...");
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
                    int id = filenameParser.ExtractIdFromFilename(filename);
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