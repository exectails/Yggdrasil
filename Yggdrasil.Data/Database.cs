// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using System;

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
	public interface IDatabaseIndexed<TIndex, TData> : IDatabase where TData : class, new()
	{
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
	}
}
