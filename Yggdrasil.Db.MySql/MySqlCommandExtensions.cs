using System;
using MySqlConnector;

namespace Yggdrasil.Db.MySql
{
	/// <summary>
	/// Extensions for MySqlCommand.
	/// </summary>
	public static class MySqlCommandExtensions
	{
		/// <summary>
		/// Shortcut for Parameters.AddWithValue, for consistency with
		/// the simplified commands.
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public static void AddParameter(this MySqlCommand cmd, string name, object value)
		{
			cmd.Parameters.AddWithValue(name, value);
		}
	}
}
