using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Yggdrasil.Data.CSV
{
	/// <summary>
	/// A text-based database using CSV.
	/// </summary>
	public abstract class DatabaseCsvBase
	{
		private readonly int _min;

		/// <summary>
		/// Warnings that occurred during load.
		/// </summary>
		protected List<DatabaseWarningException> Warnings = new List<DatabaseWarningException>();

		/// <summary>
		/// Initializes instance.
		/// </summary>
		protected DatabaseCsvBase()
		{
			var method = this.GetType().GetMethod("ReadEntry", BindingFlags.NonPublic | BindingFlags.Instance);
			var attr = method.GetCustomAttributes(typeof(MinFieldCountAttribute), true);
			if (attr.Length > 0)
				_min = (attr[0] as MinFieldCountAttribute).Count;
		}

		/// <summary>
		/// Returns warnings that occured while loading data.
		/// </summary>
		/// <returns></returns>
		public DatabaseWarningException[] GetWarnings()
		{
			lock (this.Warnings)
				return this.Warnings.ToArray();
		}

		/// <summary>
		/// Loads data from given file.
		/// </summary>
		/// <param name="filePath"></param>
		public void LoadFile(string filePath)
		{
			this.Warnings.Clear();

			using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				var csv = new CsvReader(stream, filePath, ',');
				var fileName = filePath.Replace("\\", "/");

				foreach (var entry in csv.Next())
				{
					var line = entry.Line;

					try
					{
						if (entry.Count < _min)
							throw new FieldCountException(_min, entry.Count);

						this.ReadEntry(entry);
					}
					catch (CsvDatabaseWarningException ex)
					{
						ex.Source = fileName;
						ex.Line = line;

						this.Warnings.Add(ex);
						continue;
					}
					catch (OverflowException)
					{
						var msg = string.Format("Variable not fit for type (#{0}).", entry.LastIndex);

						this.Warnings.Add(new CsvDatabaseWarningException(fileName, line, msg));
						continue;
					}
					catch (FormatException)
					{
						var msg = string.Format("Invalid number format (#{0}).", entry.LastIndex);

						this.Warnings.Add(new CsvDatabaseWarningException(fileName, line, msg));
						continue;
					}
					catch (IndexOutOfRangeException ex)
					{
						var msg = string.Format("Invalid index used at {0}", ex.StackTrace);

						this.Warnings.Add(new CsvDatabaseWarningException(fileName, line, msg));
						continue;
					}
					catch (Exception ex)
					{
						var msg = string.Format("Exception: {0}\nEntry: \n{1}", ex, line);

						throw new DatabaseErrorException(filePath, msg);
					}
				}
			}
		}

		/// <summary>
		/// Reads entry and adds information to database.
		/// </summary>
		/// <param name="entry"></param>
		protected abstract void ReadEntry(CsvEntry entry);
	}

	/// <summary>
	/// A text-based database using CSV.
	/// </summary>
	/// <typeparam name="TData"></typeparam>
	public abstract class DatabaseCsv<TData> : DatabaseCsvBase, IDatabase<TData> where TData : class, new()
	{
		/// <summary>
		/// Returns number of entries in the database.
		/// </summary>
		public int Count => this.Entries.Count;

		/// <summary>
		/// The database's entries.
		/// </summary>
		public List<TData> Entries = new List<TData>();

		/// <summary>
		/// Searches for first entry that matches the given predicate
		/// and returns it, or null if no matches were found.
		/// </summary>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public TData Find(Func<TData, bool> predicate)
		{
			TData result;
			lock (this.Entries)
				result = this.Entries.FirstOrDefault(predicate);
			return result;
		}

		/// <summary>
		/// Searches for entries that match the given predicate
		/// and returns them.
		/// </summary>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public TData[] FindAll(Func<TData, bool> predicate)
		{
			lock (this.Entries)
				return this.Entries.Where(predicate).ToArray();
		}

		/// <summary>
		/// Removes all entries from database.
		/// </summary>
		public virtual void Clear()
		{
			lock (this.Entries)
				this.Entries.Clear();
		}

		/// <summary>
		/// Adds data to database.
		/// </summary>
		/// <param name="data"></param>
		public void Add(TData data)
		{
			lock (this.Entries)
				this.Entries.Add(data);
		}
	}

	/// <summary>
	/// A text-based database using CSV.
	/// </summary>
	/// <typeparam name="TIndex"></typeparam>
	/// <typeparam name="TData"></typeparam>
	public abstract class DatabaseCsvIndexed<TIndex, TData> : DatabaseCsvBase, IDatabaseIndexed<TIndex, TData> where TData : class, new()
	{
		/// <summary>
		/// Returns number of entries in the database.
		/// </summary>
		public int Count => this.Entries.Count;

		/// <summary>
		/// The database's entries.
		/// </summary>
		public Dictionary<TIndex, TData> Entries = new Dictionary<TIndex, TData>();

		/// <summary>
		/// Returns true if the database contains an element with the
		/// given index.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public bool Contains(TIndex index)
			=> this.Entries.ContainsKey(index);

		/// <summary>
		/// Searches for first entry that matches the given predicate
		/// and returns it, or null if no matches were found.
		/// </summary>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public TData Find(Func<TData, bool> predicate)
		{
			TData result;
			lock (this.Entries)
				result = this.Entries.Values.FirstOrDefault(predicate);
			return result;
		}

		/// <summary>
		/// Returns the entry with the given index, or null if it
		/// wasn't found.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public TData Find(TIndex index)
		{
			TData result;
			lock (this.Entries)
				this.Entries.TryGetValue(index, out result);
			return result;
		}

		/// <summary>
		/// Returns the entry with the given index via out. Returns false
		/// if the index wasn't found.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public bool TryFind(TIndex index, out TData data)
		{
			lock (this.Entries)
				return this.Entries.TryGetValue(index, out data);
		}

		/// <summary>
		/// Searches for entries that match the given predicate
		/// and returns them.
		/// </summary>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public TData[] FindAll(Func<TData, bool> predicate)
		{
			lock (this.Entries)
				return this.Entries.Values.Where(predicate).ToArray();
		}

		/// <summary>
		/// Removes all entries from database.
		/// </summary>
		public virtual void Clear()
		{
			lock (this.Entries)
				this.Entries.Clear();
		}

		/// <summary>
		/// Adds data to database, fails and returns false if index exists
		/// already.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="data"></param>
		public bool Add(TIndex index, TData data)
		{
			lock (this.Entries)
			{
				if (this.Entries.ContainsKey(index))
					return false;

				this.Entries.Add(index, data);
			}

			return true;
		}

		/// <summary>
		/// Adds data to database, replacing potentially existing values.
		/// Returns whether data was replaced or not.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="data"></param>
		public bool AddOrReplace(TIndex index, TData data)
		{
			var result = false;

			lock (this.Entries)
			{
				result = this.Entries.ContainsKey(index);
				this.Entries[index] = data;
			}

			return result;
		}
	}

	/// <summary>
	/// Marks a CSV database to required at least the given amount amount
	/// of values in a line.
	/// </summary>
	public class MinFieldCountAttribute : Attribute
	{
		/// <summary>
		/// Number of required values.
		/// </summary>
		public int Count { get; protected set; }

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="count"></param>
		public MinFieldCountAttribute(int count)
		{
			this.Count = count;
		}
	}
}
