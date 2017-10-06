// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using System;
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
						var obj = entry as JObject;
						if (obj == null)
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
		protected List<TData> Entries = new List<TData>();

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
		/// Removes all entries from database.
		/// </summary>
		public virtual void Clear()
		{
			lock (this.Entries)
				this.Entries.Clear();
		}
	}

	/// <summary>
	/// A text-based database using JSON.
	/// </summary>
	/// <typeparam name="TIndex"></typeparam>
	/// <typeparam name="TData"></typeparam>
	public abstract class DatabaseJsonIndexed<TIndex, TData> : DatabaseJsonBase, IDatabaseIndexed<TIndex, TData> where TData : class, new()
	{
		protected Dictionary<TIndex, TData> Entries = new Dictionary<TIndex, TData>();

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
		/// Removes all entries from database.
		/// </summary>
		public virtual void Clear()
		{
			lock (this.Entries)
				this.Entries.Clear();
		}
	}
}
