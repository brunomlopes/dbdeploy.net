using System;
using System.Collections.Generic;
using System.IO;
using Net.Sf.Dbdeploy.Exceptions;

namespace Net.Sf.Dbdeploy.Scripts
{
    public class DirectoryScanner
    {
        private readonly FilenameParser filenameParser;

        public DirectoryScanner()
        {
            filenameParser = new FilenameParser();
        }

        public List<ChangeScript> GetChangeScriptsForDirectory(DirectoryInfo directory)
        {
            try
            {
                Console.Error.WriteLine("Reading change scripts from directory " + directory.FullName + "...");
            }
            catch (IOException)
            {
                // ignore
            }

            List<ChangeScript> scripts = new List<ChangeScript>();

            foreach (FileInfo file in directory.GetFiles())
            {
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