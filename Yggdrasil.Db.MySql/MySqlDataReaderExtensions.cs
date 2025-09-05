using System;
using MySqlConnector;

namespace Yggdrasil.Db.MySql
{
	/// <summary>
	/// Extensions for the MySqlDataReader type.
	/// </summary>
	public static class MySqlDataReaderExtensions
	{
		/// <summary>
		/// Returns the value of the named field, defaulting to the given
		/// value if the field is null.
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="field"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public static string GetStringSafe(this MySqlDataReader reader, string field, string defaultValue)
		{
			var ordinal = reader.GetOrdinal(field);
			return reader.IsDBNull(ordinal) ? defaultValue : reader.GetString(ordinal);
		}

		/// <summary>
		/// Returns the value of the named field, defaulting to the given
		/// value if the field is null.
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="field"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public static DateTime GetDateTimeSafe(this MySqlDataReader reader, string field, DateTime defaultValue)
		{
			var ordinal = reader.GetOrdinal(field);
			return reader.IsDBNull(ordinal) ? defaultValue : reader.GetDateTime(ordinal);
		}

		/// <summary>
		/// Returns the value of the named field, defaulting to the given
		/// value if the field is null.
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="field"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public static DateTime? GetOptionalDateTime(this MySqlDataReader reader, string field, DateTime? defaultValue)
		{
			var ordinal = reader.GetOrdinal(field);
			return reader.IsDBNull(ordinal) ? defaultValue : reader.GetDateTime(ordinal);
		}

		/// <summary>
		/// Returns the value of the named field, defaulting to the given
		/// value if the field is null.
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="field"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public static int? GetOptionalInt32(this MySqlDataReader reader, string field, int? defaultValue)
		{
			var ordinal = reader.GetOrdinal(field);
			return reader.IsDBNull(ordinal) ? defaultValue : reader.GetInt32(ordinal);
		}
	}
}
