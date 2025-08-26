using System;
using MySqlConnector;

namespace Yggdrasil.Db.MySql
{
	/// <summary>
	/// A base for MySQL database classes.
	/// </summary>
	public class BaseMySqlDb
	{
		private string _connectionString;

		/// <summary>
		/// Initializes database connection and establishes a test
		/// connection. Throws if connection fails.
		/// </summary>
		/// <param name="host"></param>
		/// <param name="user"></param>
		/// <param name="pass"></param>
		/// <param name="db"></param>
		public void Init(string host, string user, string pass, string db)
		{
			_connectionString = string.Format("server={0}; database={1}; uid={2}; password={3}; charset=utf8; pooling=true; min pool size=0; max pool size=100;", host, db, user, pass);
			this.TestConnection();
		}

		/// <summary>
		/// Returns a valid connection.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="Exception">
		/// Thrown if the database was not initialized.
		/// </exception>
		protected MySqlConnection GetConnection()
		{
			if (_connectionString == null)
				throw new Exception("Database has not been initialized.");

			var result = new MySqlConnection(_connectionString);
			result.Open();
			return result;
		}

		/// <summary>
		/// Establishes a connection o the database. Throws if connection
		/// fails.
		/// </summary>
		public void TestConnection()
		{
			MySqlConnection conn = null;

			try
			{
				conn = this.GetConnection();
			}
			finally
			{
				conn?.Close();
			}
		}
	}
}
