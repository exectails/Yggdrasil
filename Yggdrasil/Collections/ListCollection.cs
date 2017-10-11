// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;
using System.Linq;

namespace Yggdrasil.Collections
{
	/// <summary>
	/// Thread-safe wrapper around a generic dictionary for indexed lists.
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue">Type of the list.</typeparam>
	public class ListCollection<TKey, TValue>
	{
		protected Dictionary<TKey, List<TValue>> _entries;

		/// <summary>
		/// Returns number of lists in collection.
		/// </summary>
		public int Count { get { lock (_entries) return _entries.Count; } }

		/// <summary>
		/// Creates new collection.
		/// </summary>
		public ListCollection()
		{
			_entries = new Dictionary<TKey, List<TValue>>();
		}

		/// <summary>
		/// Returns the number of values in the list with the given key.
		/// Returns 0 if list doesn't exist.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public int CountValues(TKey key)
		{
			lock (_entries)
			{
				if (!_entries.TryGetValue(key, out var list))
					return 0;

				return list.Count;
			}
		}

		/// <summary>
		/// Adds value to key's list.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public void Add(TKey key, TValue value)
		{
			lock (_entries)
			{
				_entries.TryGetValue(key, out var list);

				if (list == null)
				{
					list = new List<TValue>();
					_entries.Add(key, list);
				}

				list.Add(value);
			}
		}

		/// <summary>
		/// Clears collection.
		/// </summary>
		public void Clear()
		{
			lock (_entries)
				_entries.Clear();
		}

		/// <summary>
		/// Clears list in collection.
		/// </summary>
		public void Clear(TKey key)
		{
			lock (_entries)
				if (_entries.ContainsKey(key))
					_entries[key].Clear();
		}

		/// <summary>
		/// Returns true if collection contains a value for key.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool ContainsKey(TKey key)
		{
			lock (_entries)
				return _entries.ContainsKey(key);
		}

		/// <summary>
		/// Returns copy of list for key, or an empty list if the key
		/// doesn't exist.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public List<TValue> Get(TKey key)
		{
			List<TValue> result;

			lock (_entries)
			{
				if (_entries.TryGetValue(key, out result))
					result = result.ToList();
			}

			return result;
		}
	}
}
