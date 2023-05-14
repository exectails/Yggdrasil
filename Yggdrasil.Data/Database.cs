using System;
using System.Collections.Generic;

namespace Yggdrasil.Data
{
	/// <summary>
	/// A database that can load data.
	/// </summary>
	public interface IDatabase
	{
		/// <summary>
		/// Returns the number of entries in the database.
		/// </summary>
		int Count { get; }

		/// <summary>
		/// Removes all entries from database.
		/// </summary>
		void Clear();

		/// <summary>
		/// Loads data from file.
		/// </summary>
		/// <param name="filePath"></param>
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

		/// <summary>
		/// Searches for entries that match the given predicate
		/// and returns them.
		/// </summary>
		/// <param name="predicate"></param>
		/// <returns></returns>
		TData[] FindAll(Func<TData, bool> predicate);

		/// <summary>
		/// Adds data to database.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		void Add(TData data);
	}

	/// <summary>
	/// A database that can load data and stores it as the given type,
	/// indexed by the given index type.
	/// </summary>
	/// <typeparam name="TIndex"></typeparam>
	/// <typeparam name="TData"></typeparam>
	public interface IDatabaseIndexed<TIndex, TData> : IDatabase where TData : class, new()
	{
		/// <summary>
		/// Returns true if the database contains an element with the
		/// given index.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		bool Contains(TIndex index);

		/// <summary>
		/// Searches for first entry that matches the given predicate
		/// and returns it, or null if no matches were found.
		/// </summary>
		/// <param name="predicate"></param>
		/// <returns></returns>
		TData Find(Func<TData, bool> predicate);

		/// <summary>
		/// Returns the entry with the given index, or null if it
		/// wasn't found.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		TData Find(TIndex index);

		/// <summary>
		/// Returns the entry with the given index via out. Returns false
		/// if the index wasn't found.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		bool TryFind(TIndex index, out TData data);

		/// <summary>
		/// Returns the first entry matching the given predicate via out.
		/// Returns false if no matches were found.
		/// </summary>
		/// <param name="predicate"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		bool TryFind(Func<TData, bool> predicate, out TData data);

		/// <summary>
		/// Searches for entries that match the given predicate
		/// and returns them.
		/// </summary>
		/// <param name="predicate"></param>
		/// <returns></returns>
		TData[] FindAll(Func<TData, bool> predicate);

		/// <summary>
		/// Adds data to database, fails and returns false if index exists
		/// already.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		bool Add(TIndex index, TData data);

		/// <summary>
		/// Adds data to database, replacing potentially existing values.
		/// Returns whether data was replaced or not.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		bool AddOrReplace(TIndex index, TData data);
	}
}
