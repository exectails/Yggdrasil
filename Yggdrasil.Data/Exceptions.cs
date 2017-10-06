// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using System;

namespace Yggdrasil.Data
{
	/// <summary>
	/// Information about a minor issue that was encountered while
	/// reading a database.
	/// </summary>
	public class DatabaseWarningException : Exception
	{
		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="msg"></param>
		public DatabaseWarningException(string msg)
			: base(msg)
		{
		}

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="source"></param>
		public DatabaseWarningException(string msg, string source)
			: this(msg)
		{
			this.Source = source;
		}

		/// <summary>
		/// Returns string representation of this exception.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return this.Source + ": " + this.Message;
		}
	}

	/// <summary>
	/// Information about a major issue that was encountered while
	/// reading a database.
	/// </summary>
	public class DatabaseErrorException : Exception
	{
		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="msg"></param>
		public DatabaseErrorException(string msg)
			: base(msg)
		{
		}

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="source"></param>
		public DatabaseErrorException(string msg, string source)
			: this(msg)
		{
			this.Source = source;
		}

		/// <summary>
		/// Returns string representation of this exception.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return this.Source + ": " + this.Message;
		}
	}
}
