using System;
using System.Collections.Generic;
using MySqlConnector;

namespace Yggdrasil.Db.MySql.SimpleCommands
{
	/// <summary>
	/// Describes a simplified MySQL command, for easier, cleaner
	/// inserting and updating.
	/// </summary>
	public interface ISimpleCommand
	{
		/// <summary>
		/// Sets value for field.
		/// </summary>
		/// <param name="field"></param>
		/// <param name="value"></param>
		void Set(string field, object value);

		/// <summary>
		/// Executes command.
		/// </summary>
		/// <returns></returns>
		int Execute();
	}

	/// <summary>
	/// Base class for simplified MySQL commands.
	/// </summary>
	public abstract class SimpleCommand : ISimpleCommand, IDisposable
	{
		/// <summary>
		/// Underlying MySqlCommand object.
		/// </summary>
		protected MySqlCommand _mc;

		/// <summary>
		/// Collection of fields to set.
		/// </summary>
		protected Dictionary<string, object> _set;

		/// <summary>
		/// Initializes internal objects.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="conn"></param>
		/// <param name="trans"></param>
		protected SimpleCommand(string command, MySqlConnection conn, MySqlTransaction trans = null)
		{
			_mc = new MySqlCommand(command, conn, trans);
			_set = new Dictionary<string, object>();
		}

		/// <summary>
		/// Adds a parameter that's not handled by Set.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public void AddParameter(string name, object value)
		{
			_mc.Parameters.AddWithValue(name, value);
		}

		/// <summary>
		/// Sets value for field.
		/// </summary>
		/// <param name="field"></param>
		/// <param name="value"></param>
		public void Set(string field, object value)
		{
			_set[field] = value;
		}

		/// <summary>
		/// Executes command.
		/// </summary>
		/// <returns></returns>
		public abstract int Execute();

		/// <summary>
		/// Disposes internal, wrapped objects.
		/// </summary>
		public void Dispose()
		{
			_mc.Dispose();
		}
	}
}
