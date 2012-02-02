using System;
using System.IO;
using System.Text;

namespace Net.Sf.Dbdeploy.Scripts
{
    public class ChangeScript : IComparable
    {
        private const string UndoToken = "--//@UNDO";

        private readonly int id;
        private readonly FileInfo file;
        private readonly String description;

        public ChangeScript(int id)
            : this(id, "test")
        {
        }

        public ChangeScript(int id, FileInfo file)
        {
            this.id = id;
            this.file = file;
            description = file.Name;
        }

        public ChangeScript(int id, String description)
        {
            this.id = id;
            file = null;
            this.description = description;
        }

        public FileInfo GetFile()
        {
            return file;
        }

        public int GetId()
        {
            return id;
        }

        public String GetDescription()
        {
            return description;
        }

        public int CompareTo(object o)
        {
            ChangeScript other = (ChangeScript) o;
            return id.CompareTo(other.id);
        }

        public virtual string GetContent()
        {
            return this.GetFileContents(undo: false);
        }

        public virtual string GetUndoContent()
        {
            return this.GetFileContents(undo: true);
        }

        private string GetFileContents(bool undo = false)
        {
            StringBuilder result = new StringBuilder();

            bool foundUndo = false;

            using (StreamReader input = File.OpenText(file.FullName))
            {
                string str;
                while ((str = input.ReadLine()) != null)
                {
                    if (str == null)
                        continue;

                    // Just keep looping until we find the magic "--//@UNDO"
                    if (UndoToken == str.Trim())
                    {
                        foundUndo = true;                        
                        continue;
                    }

                    if (foundUndo == undo)
                    {
                        result.AppendLine(str);
                    }
                }
            }

            return result.ToString();
        }

        public override string ToString()
        {
            if (file != null)
                return "#" + id + ": " + file.Name;

            return "#" + id;
        }
    }
}