using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySqlConnector;

namespace Yggdrasil.Db.MySql.SimpleCommands
{
	/// <summary>
	/// Optimized batch insert command, using MySqlBulkCopy under the
	/// hood.
	/// </summary>
	/// <remarks>
	/// This is not intended as a general-purpose command, but rather for
	/// specific cases where a large number of rows needs to be inserted
	/// at once. For simple inserts, use <see cref="InsertCommand"/>
	/// instead.
	///
	/// Note that loading local files needs to be enabled on the database
	/// server for this command to work. This can be done via the setting
	/// `local_infile` or the `AllowLoadLocalInfile` connection string.
	///
	/// If local infile loading is not an option, consider using
	/// <see cref="BatchedInsertCommand"/>.
	/// </remarks>
	/// <example>
	/// <code>
	/// using (var cmd = new FastBatchedInsertCommand("keywords", conn, transaction))
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
	public class FastBatchedInsertCommand : ISimpleCommand, IDisposable
	{
		/// <summary>
		/// Name of the table to insert into.
		/// </summary>
		protected string _tableName;

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
		public FastBatchedInsertCommand(string tableName, MySqlConnection conn, MySqlTransaction transaction = null)
		{
			_tableName = tableName;
			_conn = conn;
			_transaction = transaction;

			_rows = new List<Dictionary<string, object>>();
			_rows.Add(_curRow = new Dictionary<string, object>());
		}

		/// <summary>
		/// Cleans up internal objects.
		/// </summary>
		public void Dispose()
		{
			_tableName = null;
			_conn = null;
			_transaction = null;
			_rows = null;
			_curRow = null;
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
			_rows.Clear();
			_curRow.Clear();

			_rows.Add(_curRow);
		}

		/// <summary>
		/// Executes a batched insert based on the values given and
		/// returns the number of rows inserted.
		/// </summary>
		/// <returns></returns>
		public int Execute()
		{
			if (_rows.Count == 0 || _rows[0].Count == 0)
				return 0;

			var dataTable = new DataTable();
			var columnNames = _rows[0].Keys.ToArray();

			// Define columns
			foreach (var name in columnNames)
			{
				// Determine type based on the first value
				var firstNonNullValue = _rows.Select(a => a.TryGetValue(name, out var val) ? val : null).FirstOrDefault(v => v != null);
				var colType = firstNonNullValue != null ? firstNonNullValue.GetType() : typeof(object);

				colType = Nullable.GetUnderlyingType(colType) ?? colType;
				dataTable.Columns.Add(name, colType);
			}

			// Add data values
			foreach (var row in _rows)
			{
				if (row.Count == 0)
					continue;

				var dataRow = dataTable.NewRow();

				foreach (var name in columnNames)
				{
					if (!row.TryGetValue(name, out var val) || val == null)
					{
						dataRow[name] = DBNull.Value;
						continue;
					}

					dataRow[name] = val;
				}

				dataTable.Rows.Add(dataRow);
			}

			// Let it rip
			var bulkCopy = new MySqlBulkCopy(_conn, _transaction)
			{
				DestinationTableName = _tableName,
				BulkCopyTimeout = 30,
			};

			var result = bulkCopy.WriteToServer(dataTable);

			return result.RowsInserted;
		}
	}
}
