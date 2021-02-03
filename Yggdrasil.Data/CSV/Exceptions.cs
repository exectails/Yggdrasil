using System;

namespace Yggdrasil.Data.CSV
{
	/// <summary>
	/// A minor issue encountered while reading data from a CSV database.
	/// </summary>
	public class CsvDatabaseWarningException : DatabaseWarningException
	{
		/// <summary>
		/// The line the issue occurred on.
		/// </summary>
		public int Line { get; internal set; }

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="line"></param>
		/// <param name="msg"></param>
		public CsvDatabaseWarningException(string source, int line, string msg)
			: base(source, msg)
		{
			this.Line = line;
		}

		/// <summary>
		/// Returns string representation of this exception.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format("{0} on line {1}: {2}", this.Source, this.Line, this.Message);
		}
	}

	/// <summary>
	/// A minor issue where a CSV line didn't have the expected amount
	/// of values.
	/// </summary>
	public class FieldCountException : CsvDatabaseWarningException
	{
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="expectedAmount"></param>
		/// <param name="amount"></param>
		public FieldCountException(int expectedAmount, int amount)
			: base(null, 0, string.Format("Expected at least {0} fields, found {1}.", expectedAmount, amount))
		{
		}
	}
}
