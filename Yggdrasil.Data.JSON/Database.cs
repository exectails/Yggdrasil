﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yggdrasil.Data.JSON
{
	/// <summary>
	/// A text-based database using JSON.
	/// </summary>
	public abstract class DatabaseJsonBase
	{
		/// <summary>
		/// Warnings that occurred during loading.
		/// </summary>
		protected List<DatabaseWarningException> Warnings = new List<DatabaseWarningException>();

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

			using (var fs = new StreamReader(filePath))
			using (var reader = new JsonTextReader(fs))
			{
				try
				{
					var array = JArray.Load(reader);

					foreach (var entry in array)
					{
						if (!(entry is JObject obj))
							continue;

						try
						{
							this.ReadEntry(obj);
						}
						catch (MandatoryValueException ex)
						{
							this.Warnings.Add(new MandatoryValueException(filePath, ex.Key, obj));
							continue;
						}
						catch (DatabaseWarningException ex)
						{
							var msg = string.Format("{0}\n{1}", ex.Message, obj);

							this.Warnings.Add(new DatabaseWarningException(filePath, msg));
							continue;
						}
						catch (OverflowException)
						{
							var msg = string.Format("Number too big or too small for variable, in \n{0}", obj);

							this.Warnings.Add(new DatabaseWarningException(filePath, msg));
							continue;
						}
						catch (Exception ex)
						{
							var msg = string.Format("Exception: {0}\nEntry: \n{1}", ex, obj);

							throw new DatabaseErrorException(filePath, msg);
						}
					}
				}
				catch (JsonReaderException ex)
				{
					// Throw to stop the server, databases depend on each
					// other, skipping one could lead to problems.
					throw new DatabaseErrorException(filePath, ex.Message);
				}
			}

			this.AfterLoad();
		}

		/// <summary>
		/// Called at the end of LoadFile.
		/// </summary>
		protected virtual void AfterLoad()
		{
		}

		/// <summary>
		/// Reads entry and adds information to database.
		/// </summary>
		/// <param name="entry"></param>
		protected abstract void ReadEntry(JObject entry);
	}

	/// <summary>
	/// A text-based database using JSON.
	/// </summary>
	/// <typeparam name="TData"></typeparam>
	public abstract class DatabaseJson<TData> : DatabaseJsonBase, IDatabase<TData> where TData : class, new()
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
	/// A text-based database using JSON.
	/// </summary>
	/// <typeparam name="TIndex"></typeparam>
	/// <typeparam name="TData"></typeparam>
	public abstract class DatabaseJsonIndexed<TIndex, TData> : DatabaseJsonBase, IDatabaseIndexed<TIndex, TData> where TData : class, new()
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
		/// Returns the first entry matching the given predicate via out.
		/// Returns false if no matches were found.
		/// </summary>
		/// <param name="predicate"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public bool TryFind(Func<TData, bool> predicate, out TData data)
		{
			data = default;

			lock (this.Entries)
				data = this.Entries.Values.FirstOrDefault(predicate);

			return data != null;
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
}
