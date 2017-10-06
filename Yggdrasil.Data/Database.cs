// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Yggdrasil.Data
{
	/// <summary>
	/// A database that can load data.
	/// </summary>
	public interface IDatabase
	{
		/// <summary>
		/// Removes all entries from database.
		/// </summary>
		void Clear();

		/// <summary>
		/// Loads data from file.
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="clear"></param>
		void LoadFile(string filePath);

		/// <summary>
		/// Returns warnings that occurred while loading data.
		/// </summary>
		/// <returns></returns>
		DatabaseWarningException[] GetWarnings();
	}

	/// <summary>
	/// A database that can load data and stores it as the given type.
	/// </summary>
	/// <typeparam name="TData"></typeparam>
	public interface IDatabase<TData> : IDatabase where TData : class, new()
	{
		/// <summary>
		/// Searches for first entry that matches the given predicate
		/// and returns it, or null if no matches were found.
		/// </summary>
		/// <param name="predicate"></param>
		/// <returns></returns>
		TData Find(Func<TData, bool> predicate);
	}

	/// <summary>
	/// A database that can load data and stores it as the given type,
	/// indexed by the given index type.
	/// </summary>
	/// <typeparam name="TIndex"></typeparam>
	/// <typeparam name="TData"></typeparam>
	public interface IIndexedDatabase<TIndex, TData> : IDatabase<TData> where TData : class, new()
	{
		/// <summary>
		/// Returns the entry with the given index, or null if it
		/// wasn't found.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		TData Find(TIndex index);
	}

	/// <summary>
	/// Base implemention of IDatabase for the given type.
	/// </summary>
	/// <typeparam name="TData"></typeparam>
	public abstract class Database<TData> : IDatabase<TData> where TData : class, new()
	{
		protected List<TData> Entries = new List<TData>();

		protected List<DatabaseWarningException> Warnings = new List<DatabaseWarningException>();

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

		/// <summary>
		/// Loads data from given file using a stream.
		/// </summary>
		/// <param name="filePath"></param>
		public abstract void LoadFile(string filePath);

		/// <summary>
		/// Returns warnings that occured while loading data.
		/// </summary>
		/// <returns></returns>
		public DatabaseWarningException[] GetWarnings()
		{
			lock (this.Warnings)
				return this.Warnings.ToArray();
		}
	}

	/// <summary>
	/// Base implemention of IIndexedDatabase for the given types.
	/// </summary>
	/// <typeparam name="TIndex"></typeparam>
	/// <typeparam name="TData"></typeparam>
	public abstract class IndexedDatabase<TIndex, TData> : IIndexedDatabase<TIndex, TData> where TData : class, new()
	{
		protected Dictionary<TIndex, TData> Entries = new Dictionary<TIndex, TData>();
		protected List<DatabaseWarningException> Warnings = new List<DatabaseWarningException>();

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

		/// <summary>
		/// Loads data from given file using a stream.
		/// </summary>
		/// <param name="filePath"></param>
		public abstract void LoadFile(string filePath);

		/// <summary>
		/// Returns warnings that occured while loading data.
		/// </summary>
		/// <returns></returns>
		public DatabaseWarningException[] GetWarnings()
		{
			lock (this.Warnings)
				return this.Warnings.ToArray();
		}
	}
}
