using System;
using System.Collections.Generic;
using System.IO;
using Net.Sf.Dbdeploy.Exceptions;
using System.Text;

namespace Net.Sf.Dbdeploy.Scripts
{
    using System.Linq;

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

            // Order scripts by number, since they may not have leading zeros on file system.
            // This allows us to have more than a thousand scripts (001) in a directory, and it does not have to be specified beforehand.
            scripts = scripts.OrderBy(s => s.GetId()).ToList();

            return scripts;
        }
    }
}