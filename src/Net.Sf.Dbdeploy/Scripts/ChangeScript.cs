using System;
using System.IO;

namespace Net.Sf.Dbdeploy.Scripts
{
    public class ChangeScript : IComparable
    {
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

        public override string ToString()
        {
            if (file != null)
                return "#" + id + ": " + file.Name;

            return "#" + id;
        }
    }
}