using System;
using System.IO;
using System.Text;
using Microsoft.Build.Framework;
using Net.Sf.Dbdeploy;
using Net.Sf.Dbdeploy.Database;
using Net.Sf.Dbdeploy.Exceptions;

namespace MSBuild.Dbdeploy.Task
{
	public class Dbdeploy : Microsoft.Build.Utilities.Task
	{
		private string dbType;
		private string dbConnection;
		private DirectoryInfo dir;
		private FileInfo outputfile;
		private FileInfo undoOutputfile;
		private int lastChangeToApply = Int32.MaxValue;
		private string deltaSet = "Main";
		private bool useTransaction = false;

		[Required]
		public string DbType
		{
			set { dbType = value; }
		}

		[Required]
		public string DbConnection
		{
			set { dbConnection = value; }
		}

		[Required]
		public string Dir
		{
			get { return dir.FullName; }
			set { dir = new DirectoryInfo(value); }
		}

		[Required]
		public string OutputFile
		{
			get { return outputfile.FullName; }
			set { outputfile = new FileInfo(value); }
		}

		public string UndoOutputFile
		{
			get { return undoOutputfile.FullName; }
			set { undoOutputfile = new FileInfo(value); }
		}

		public int LastChangeToApply
		{
			get { return lastChangeToApply; }
			set { lastChangeToApply = value; }
		}

		public string DeltaSet
		{
			get { return deltaSet; }
			set { deltaSet = value; }
		}

		public bool UseTransaction
		{
			get { return useTransaction; }
			set { useTransaction = value; }
		}

		public override bool Execute()
		{
			bool result = false;
			try
			{
				LogTaskProperties();

			    using (TextWriter outputPrintStream = new StreamWriter(outputfile.FullName))
				{
					TextWriter undoOutputPrintStream = null;
					if (undoOutputfile != null)
					{
						undoOutputPrintStream = new StreamWriter(undoOutputfile.FullName);
					}
					DbmsFactory factory = new DbmsFactory(dbType, dbConnection);
					IDbmsSyntax dbmsSyntax = factory.CreateDbmsSyntax();
					DatabaseSchemaVersionManager databaseSchemaVersion = new DatabaseSchemaVersionManager(factory, deltaSet, null);

					ToPrintStreamDeployer toPrintSteamDeployer = new ToPrintStreamDeployer(databaseSchemaVersion, dir, outputPrintStream, dbmsSyntax, useTransaction, undoOutputPrintStream);
					toPrintSteamDeployer.DoDeploy(lastChangeToApply);

					if (undoOutputPrintStream != null)
					{
						undoOutputPrintStream.Close();
					}
				}
				result = true;
			}
			catch (DbDeployException ex)
			{
				Log.LogErrorFromException(ex, true);
				Console.Error.WriteLine(ex.Message);
			}
			catch (Exception ex)
			{
				Log.LogErrorFromException(ex, true);
				Console.Error.WriteLine("Failed to apply changes: " + ex.Message);
				Console.Error.WriteLine("Stack Trace:");
				Console.Error.Write(ex.StackTrace);
			}
			return result;
		}

	    private void LogTaskProperties()
	    {
	        if (BuildEngine == null)
                return;

	        StringBuilder builder = new StringBuilder();
	        builder.Append("DbType=");
	        builder.Append(dbType);
	        builder.AppendLine();
	        builder.Append("DbConnection=");
	        builder.Append(dbConnection);
	        builder.AppendLine();
	        builder.Append("Dir=");
	        builder.Append(dir);
	        builder.AppendLine();
	        builder.Append("OutputFile=");
	        builder.Append(outputfile);
	        builder.AppendLine();
	        builder.Append("UndoOutputFile=");
	        builder.Append(undoOutputfile);
	        builder.AppendLine();
	        builder.Append("LastChangeToApply=");
	        builder.Append(lastChangeToApply);
	        builder.AppendLine();
	        builder.Append("DeltaSet=");
	        builder.Append(deltaSet);
	        builder.AppendLine();
                
	        Log.LogMessage(builder.ToString());
	    }
	}
}