using System.Linq;
using System.Text;
using MySqlConnector;

namespace Yggdrasil.Db.MySql.SimpleCommands
{
	/// <summary>
	/// Wrapper around MySqlCommand, for easier, cleaner inserting.
	/// </summary>
	/// <remarks>
	/// Include a {parameters} where the "(...) VALUES (...) part normally
	/// would be. They're inserted automatically based on the values given
	/// via "Set".
	/// </remarks>
	/// <example>
	/// <code>
	/// using (var cmd = new InsertCommand("INSERT INTO `keywords` {parameters}", conn, transaction))
	/// {
	/// 	cmd.Set("creatureId", creature.CreatureId);
	/// 	cmd.Set("keywordId", keywordId);
	/// 
	/// 	cmd.Execute();
	/// }
	/// </code>
	/// </example>
	public class InsertCommand : SimpleCommand
	{
		/// <summary>
		/// Returns last insert id.
		/// </summary>
		public long LastId { get { return _mc.LastInsertedId; } }

		/// <summary>
		/// Creates new insert command.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="conn"></param>
		/// <param name="transaction"></param>
		public InsertCommand(string command, MySqlConnection conn, MySqlTransaction transaction = null)
			: base(command, conn, transaction)
		{
		}

		/// <summary>
		/// Builds the parameterized query that is executed by this
		/// command.
		/// </summary>
		/// <returns></returns>
		public string Build()
		{
			var sb1 = new StringBuilder();
			var sb2 = new StringBuilder();

			foreach (var parameter in _set.Keys)
			{
				sb1.AppendFormat("`{0}`, ", parameter);
				sb2.AppendFormat("@{0}, ", parameter);
			}

			var columns = sb1.ToString(0, sb1.Length - 2);
			var values = sb2.ToString(0, sb2.Length - 2);

			var parameters = "(" + columns + ") VALUES (" + values + ")";
			return _mc.CommandText.Replace("{parameters}", parameters);
		}

		/// <summary>
		/// Runs MySqlCommand.ExecuteNonQuery
		/// </summary>
		/// <returns></returns>
		public override int Execute()
		{
			var query = this.Build();

			_mc.CommandText = query;

			foreach (var parameter in _set)
				_mc.Parameters.AddWithValue("@" + parameter.Key, parameter.Value);

			return _mc.ExecuteNonQuery();
		}
	}
}
