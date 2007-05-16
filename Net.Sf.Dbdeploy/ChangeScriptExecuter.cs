using System;
using System.Globalization;
using System.IO;
using Net.Sf.Dbdeploy.Database;
using Net.Sf.Dbdeploy.Scripts;

namespace Net.Sf.Dbdeploy
{
    public class ChangeScriptExecuter
    {
        private TextWriter output;

        public ChangeScriptExecuter(TextWriter printStream, DbmsSyntax dbmsSyntax)
        {
            output = printStream;
            /* Header data: information and control settings for the entire script. */
            DateTime now = DateTime.Now;
            output.WriteLine("-- Script generated at " + now.ToString(new DateTimeFormatInfo().SortableDateTimePattern));
            output.WriteLine();
            output.WriteLine(dbmsSyntax.GenerateScriptHeader());
        }

        public void ApplyChangeDoScript(ChangeScript script)
        {
            output.WriteLine();
            output.WriteLine("-- Change script: " + script);
            CopyFileDoContentsToStdOut(script.GetFile());
        }

        public void ApplyChangeUndoScript(ChangeScript script)
        {
            output.WriteLine();
            output.WriteLine("-- Change script: " + script);
            CopyFileUndoContentsToStdOut(script.GetFile());
        }

        private void CopyFileDoContentsToStdOut(FileInfo file)
        {
            using (StreamReader input = File.OpenText(file.FullName))
            {
                string str;
                while ((str = input.ReadLine()) != null && !IsUndoToken(str))
                {
                    output.WriteLine(str);
                }
            }
        }

        private bool IsUndoToken(string text)
        {
            return "--//@UNDO" == text.Trim();
        }

        private void CopyFileUndoContentsToStdOut(FileInfo file)
        {
            using (StreamReader input = File.OpenText(file.FullName))
            {
                string str;
                while ((str = input.ReadLine()) != null)
                {
                    if (IsUndoToken(str))
                    {
                        // Just keep looping until we find the magic "--//@UNDO"
                        break;
                    }
                }
                while ((str = input.ReadLine()) != null)
                {
                    output.WriteLine(str);
                }
            }
        }

        /* Should be called *after* insert of script contents. */

        public void ApplyDeltaFragmentHeaderOrFooterSql(string sql)
        {
            output.WriteLine();
            output.WriteLine(sql);
        }
    }
}
