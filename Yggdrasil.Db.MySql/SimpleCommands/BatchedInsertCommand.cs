using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySqlConnector;

namespace Yggdrasil.Db.MySql.SimpleCommands
{
	/// <summary>
	/// Wrapper around MySqlCommand, for easier, cleaner batch inserting.
	/// </summary>
	/// <remarks>
	/// Uses multi-row insert syntax to insert multiple rows at once,
	/// which is more efficient than running multiple single-row insert
	/// commands. For simple inserts, use <see cref="InsertCommand"/>
	/// instead.
	///
	/// It's best to batch execute in regular intervals, e.g. every 1000
	/// rows, to avoid creating too large queries. While looping over data,
	/// call <see cref="Execute"/> and use <see cref="Clear"/> to insert
	/// and then clear the current batch and start over.
	/// </remarks>
	/// <example>
	/// <code>
	/// using (var cmd = new BatchedInsertCommand("keywords", conn, transaction))
	/// {
	///		foreach (var keywordId in keywordIds)
	///		{
	///			cmd.Set("creatureId", creature.CreatureId);
	///			cmd.Set("keywordId", keywordId);
	///			
	///			cmd.AddRow();
	///		}
	///		
	///		cmd.Execute();
	/// }
	/// </code>
	/// </example>
	public class BatchedInsertCommand : ISimpleCommand, IDisposable
	{
		/// <summary>
		/// Base query for the insert command.
		/// </summary>
		protected string _baseQuery;

		/// <summary>
		/// Underlying MySqlCommand object.
		/// </summary>
		protected MySqlCommand _mc;

		/// <summary>
		/// The connection to use.
		/// </summary>
		protected MySqlConnection _conn;

		/// <summary>
		/// The transaction to use, if any.
		/// </summary>
		protected MySqlTransaction _transaction;

		/// <summary>
		/// Collection of rows to set.
		/// </summary>
		protected List<Dictionary<string, object>> _rows;

		/// <summary>
		/// Collection of fields to set in the current row.
		/// </summary>
		protected Dictionary<string, object> _curRow;

		/// <summary>
		/// Creates new insert command.
		/// </summary>
		/// <param name="tableName"></param>
		/// <param name="conn"></param>
		/// <param name="transaction"></param>
		public BatchedInsertCommand(string tableName, MySqlConnection conn, MySqlTransaction transaction = null)
		{
			_baseQuery = "INSERT INTO `" + tableName + "` {parameters}";
			_mc = new MySqlCommand(_baseQuery, conn, transaction);

			_curRow = new Dictionary<string, object>();
			_rows = new List<Dictionary<string, object>> { _curRow };
		}

		/// <summary>
		/// Disposes internal, wrapped objects.
		/// </summary>
		public void Dispose()
		{
			_mc.Dispose();

			_baseQuery = null;
			_mc = null;
			_conn = null;
			_transaction = null;
			_rows = null;
			_curRow = null;
		}

		/// <summary>
		/// Builds the parameterized query that is executed by this
		/// command.
		/// </summary>
		/// <returns></returns>
		public string Build()
		{
			if (_rows.Count == 0 || _rows[0].Count == 0)
				throw new InvalidOperationException("No values were set for this command.");

			var sb1 = new StringBuilder();
			var sb2 = new StringBuilder();

			var columnNames = _rows[0].Keys.ToArray();

			for (var i = 0; i < columnNames.Length; ++i)
			{
				var name = columnNames[i];

				sb1.AppendFormat("`{0}`", name);

				if (i < columnNames.Length - 1)
					sb1.Append(", ");
			}

			for (var i = 0; i < _rows.Count; ++i)
			{
				var row = _rows[i];

				if (row.Count == 0)
					continue;

				sb2.Append("(");

				for (var j = 0; j < columnNames.Length; ++j)
				{
					var name = columnNames[j];

					sb2.AppendFormat("@{0}_" + i, name);

					if (j < columnNames.Length - 1)
						sb2.Append(", ");
				}

				sb2.Append("), ");
			}

			var columns = sb1.ToString();
			var values = sb2.ToString(0, sb2.Length - 2);

			var parameters = "(" + columns + ") VALUES " + values;
			return _baseQuery.Replace("{parameters}", parameters);
		}

		/// <summary>
		/// Sets value for field.
		/// </summary>
		/// <param name="field"></param>
		/// <param name="value"></param>
		public void Set(string field, object value)
		{
			_curRow[field] = value;
		}

		/// <summary>
		/// Adds another row to set values for. Doesn't do anything if
		/// no values were set for the current row.
		/// </summary>
		/// <example>
		/// for (var i = 0; i &lt; 5; ++i)
		/// {
		/// 	cmd.AddRow();
		/// 
		/// 	cmd.Set("id", 5 + i);
		/// 	cmd.Set("name", "foobar" + i);
		/// }
		/// </example>
		public void AddRow()
		{
			if (_curRow.Count == 0)
				return;

			_rows.Add(_curRow = new Dictionary<string, object>());
		}

		/// <summary>
		/// Clears the current values to start over with a new batch.
		/// </summary>
		public void Clear()
		{
			_curRow.Clear();

			_rows.Clear();
			_rows.Add(_curRow);

			_mc.Parameters.Clear();
		}

		/// <summary>
		/// Inserts the current batch and clears the values if the batch
		/// size is greater than or equal to the given amount.
		/// </summary>
		/// <param name="amount"></param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public void ExecuteOn(int amount)
		{
			if (amount <= 0)
				throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than 0.");

			// Calculate actual row count to support calling AddRow at the
			// top or the bottom of the loop.
			var actualRowCount = _rows.Count - (_curRow.Count == 0 ? 1 : 0);

			if (actualRowCount >= amount)
			{
				this.Execute();
				this.Clear();
			}
		}

		/// <summary>
		/// Runs MySqlCommand.ExecuteNonQuery. Does nothing if no values
		/// were set.
		/// </summary>
		/// <returns></returns>
		public int Execute()
		{
			if (_rows.Count == 0 || _rows[0].Count == 0)
				return 0;

			var query = this.Build();
			_mc.CommandText = query;

			for (var i = 0; i < _rows.Count; ++i)
			{
				var row = _rows[i];

				foreach (var parameter in row)
					_mc.Parameters.AddWithValue("@" + parameter.Key + "_" + i, parameter.Value);
			}

			return _mc.ExecuteNonQuery();
		}
	}
}
