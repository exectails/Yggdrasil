using System.Linq;
using System.Text;
using MySqlConnector;

namespace Yggdrasil.Db.MySql.SimpleCommands
{
	/// <summary>
	/// Wrapper around MySqlCommand, for easier, cleaner updating.
	/// </summary>
	/// <remarks>
	/// Include one {parameters} where the set statements normally would be.
	/// They're inserted automatically based on the values given via "Set".
	/// </remarks>
	/// <example>
	/// <code>
	/// using (var conn = db.Instance.Connection)
	/// using (var cmd = new UpdateCommand("UPDATE `accounts` SET {parameters} WHERE `accountId` = @accountId", conn))
	/// {
	/// 	cmd.AddParameter("@accountId", account.Id);
	/// 	cmd.Set("authority", (byte)account.Authority);
	/// 	cmd.Set("lastlogin", account.LastLogin);
	/// 	cmd.Set("banReason", account.BanReason);
	/// 	cmd.Set("banExpiration", account.BanExpiration);
	/// 
	/// 	cmd.Execute();
	/// }
	/// </code>
	/// </example>
	public class UpdateCommand : SimpleCommand
	{
		/// <summary>
		/// Creates new update command.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="conn"></param>
		/// <param name="trans"></param>
		public UpdateCommand(string command, MySqlConnection conn, MySqlTransaction trans = null)
			: base(command, conn, trans)
		{
		}

		/// <summary>
		/// Builds the parameterized query that is executed by this
		/// command.
		/// </summary>
		/// <returns></returns>
		public string Build()
		{
			var sb = new StringBuilder();

			foreach (var parameter in _set.Keys)
				sb.AppendFormat("`{0}` = @{0}, ", parameter);

			var values = sb.ToString(0, sb.Length - 2);

			return _mc.CommandText.Replace("{parameters}", values);
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
