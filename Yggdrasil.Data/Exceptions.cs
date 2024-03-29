﻿using System;

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
		/// <param name="source"></param>
		/// <param name="msg"></param>
		public DatabaseWarningException(string source, string msg)
			: base(msg)
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
			: this(null, msg)
		{
		}

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="msg"></param>
		public DatabaseErrorException(string source, string msg)
			: base(msg)
		{
			this.Source = source;
		}

		/// <summary>
		/// Returns string representation of this exception.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			if (string.IsNullOrWhiteSpace(this.Source))
				return this.Message;
			else
				return this.Source + ": " + this.Message;
		}
	}
}
