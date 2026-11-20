using System;
using System.Globalization;

namespace Net.Sf.Dbdeploy.Database
{
	/// <summary>
	/// IBM DB2 syntax.
	/// </summary>
	public class Db2DbmsSyntax : DbmsSyntax
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Db2DbmsSyntax" /> class.
		/// </summary>
		public Db2DbmsSyntax()
			: base("db2")
		{
		}

		/// <summary>
		/// Gets the get timestamp.
		/// </summary>
		/// <value>
		/// The get timestamp.
		/// </value>
		public override string CurrentTimestamp
		{
			get
			{
				return "CURRENT TIMESTAMP";
			}
		}

		/// <summary>
		/// Gets the syntax to get the current user.
		/// </summary>
		/// <value>
		/// The current user syntax.
		/// </value>
		public override string CurrentUser
		{
			get
			{
				return "CURRENT USER";
			}
		}

		/// <summary>
		/// Gets the syntax for checking if a table exists.
		/// </summary>
		/// <param name="tableName">Name of the table.</param>
		/// <returns>SQL for checking if a table exists.</returns>
		public override string TableExists(string tableName)
		{
			// Use correct syntax for with and without schema.
			string syntax;
			var tableInfo = this.GetTableInfo(tableName);
			if (!string.IsNullOrWhiteSpace(tableInfo.Schema))
			{
				syntax = string.Format(CultureInfo.InvariantCulture,
@"SELECT TRIM(TABSCHEMA) || '.' || TRIM(TABNAME) 
FROM SYSCAT.TABLES 
WHERE TABSCHEMA='{0}' AND TABNAME='{1}'", tableInfo.Schema.ToUpper(), tableInfo.TableName.ToUpper());
			}
			else
			{
				syntax = string.Format(CultureInfo.InvariantCulture,
@"SELECT TRIM(TABSCHEMA) || '.' || TRIM(TABNAME) 
FROM SYSCAT.TABLES 
WHERE TABNAME='{0}'", tableName.ToUpper());
			}

			return syntax;
		}
	}
}