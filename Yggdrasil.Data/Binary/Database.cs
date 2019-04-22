using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Yggdrasil.Data.Binary
{
	/// <summary>
	/// A compressed file database that stores binary data.
	/// </summary>
	public abstract class DatabaseBinaryBase
	{
		/// <summary>
		/// Warnings that occurred during load.
		/// </summary>
		protected List<DatabaseWarningException> Warnings = new List<DatabaseWarningException>();

		/// <summary>
		/// Returns all warnings that occured while loading the database.
		/// </summary>
		/// <returns></returns>
		public DatabaseWarningException[] GetWarnings()
		{
			lock (this.Warnings)
				return this.Warnings.ToArray();
		}

		/// <summary>
		/// Loads the database from the given file.
		/// </summary>
		/// <param name="filePath"></param>
		public void LoadFile(string filePath)
		{
			lock (this.Warnings)
				this.Warnings.Clear();

			var fileName = filePath.Replace("\\", "/");
			byte[] content;

			try
			{
				using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
				using (var uncompressed = new MemoryStream())
				using (var gzip = new GZipStream(fs, CompressionMode.Decompress))
				{
					gzip.CopyTo(uncompressed);
					content = uncompressed.ToArray();
				}

				using (var ms = new MemoryStream(content))
				using (var br = new BinaryReader(ms))
				{
					try
					{
						this.Read(br);
					}
					catch (DatabaseWarningException ex)
					{
						lock (this.Warnings)
							this.Warnings.Add(ex);
					}
				}
			}
			catch (InvalidDataException)
			{
				throw new DatabaseErrorException(fileName, "Failed to read data, the file might be corrupted.");
			}
		}

		/// <summary>
		/// Reads entries from binary reader.
		/// </summary>
		/// <param name="br"></param>
		protected abstract void Read(BinaryReader br);
	}

	/// <summary>
	/// A compressed file database that stores binary data.
	/// </summary>
	/// <typeparam name="TData"></typeparam>
	public abstract class DatabaseBinary<TData> : DatabaseBinaryBase, IDatabase<TData> where TData : class, new()
	{
		/// <summary>
		/// The database's entries.
		/// </summary>
		public List<TData> Entries = new List<TData>();

		/// <summary>
		/// Returns the number of entries in the database.
		/// </summary>
		public int Count => this.Entries.Count;

		/// <summary>
		/// Adds the entry to the database.
		/// </summary>
		/// <param name="data"></param>
		public void Add(TData data)
		{
			lock (this.Entries)
				this.Entries.Add(data);
		}

		/// <summary>
		/// Removes all entries from the database.
		/// </summary>
		public void Clear()
		{
			lock (this.Entries)
				this.Entries.Clear();
		}

		/// <summary>
		/// Returns the first entry that matches the given predicate.
		/// </summary>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public TData Find(Func<TData, bool> predicate)
		{
			lock (this.Entries)
			{
				return this.Entries.FirstOrDefault(predicate);
			}
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
	}

	/// <summary>
	/// A compressed file database that stores binary data.
	/// </summary>
	/// <typeparam name="TIndex"></typeparam>
	/// <typeparam name="TData"></typeparam>
	public abstract class DatabaseBinaryIndexed<TIndex, TData> : DatabaseBinaryBase, IDatabaseIndexed<TIndex, TData> where TData : class, new()
	{
		/// <summary>
		/// The database's entries.
		/// </summary>
		public Dictionary<TIndex, TData> Entries = new Dictionary<TIndex, TData>();

		/// <summary>
		/// Returns the number of entries in the database.
		/// </summary>
		public int Count => this.Entries.Count;

		/// <summary>
		/// Adds the entry to the database if it doesn't exist yet.
		/// Returns true if it was added succesfully.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public bool Add(TIndex index, TData data)
		{
			lock (this.Entries)
			{
				if (this.Entries.ContainsKey(index))
					return false;

				this.Entries.Add(index, data);
				return true;
			}
		}

		/// <summary>
		/// Adds the entry to the database. If the index already exists
		/// the entry is replaced. Returns whether data was replaced or not.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public bool AddOrReplace(TIndex index, TData data)
		{
			lock (this.Entries)
			{
				var result = this.Entries.ContainsKey(index);
				this.Entries[index] = data;
				return result;
			}
		}

		/// <summary>
		/// Removes all entries from the database.
		/// </summary>
		public void Clear()
		{
			lock (this.Entries)
				this.Entries.Clear();
		}

		/// <summary>
		/// Returns true if the database contains an element with the
		/// given index.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public bool Contains(TIndex index)
			=> this.Entries.ContainsKey(index);

		/// <summary>
		/// Returns the first entry that matches the given predicate.
		/// </summary>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public TData Find(Func<TData, bool> predicate)
		{
			lock (this.Entries)
			{
				return this.Entries.Values.FirstOrDefault(predicate);
			}
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
		/// Returns entry with the given index if it exists, or null if
		/// it doesn't.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public TData Find(TIndex index)
		{
			lock (this.Entries)
			{
				if (this.Entries.TryGetValue(index, out var data))
					return data;

				return null;
			}
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
	}
}
