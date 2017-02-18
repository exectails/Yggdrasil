// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;

namespace Yggdrasil.Collections
{
	/// <summary>
	/// Thread-safe wrapper around a generic dictionary.
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	public class Collection<TKey, TValue>
	{
		protected Dictionary<TKey, TValue> _entries;

		/// <summary>
		/// Returns number of entries in collection.
		/// </summary>
		public int Count { get { lock (_entries) return _entries.Count; } }

		/// <summary>
		/// Creates new collection.
		/// </summary>
		public Collection()
		{
			_entries = new Dictionary<TKey, TValue>();
		}

		/// <summary>
		/// Adds value to collection, returns false if key existed already.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool Add(TKey key, TValue value)
		{
			lock (_entries)
			{
				if (_entries.ContainsKey(key))
					return false;

				_entries.Add(key, value);
				return true;
			}
		}

		/// <summary>
		/// Adds value to collection, overrides existing keys.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public void Set(TKey key, TValue value)
		{
			lock (_entries)
				_entries[key] = value;
		}

		/// <summary>
		/// Removes value with key from collection, returns true if successful.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool Remove(TKey key)
		{
			lock (_entries)
				return _entries.Remove(key);
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
		/// Returns true if collection contains the value.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool ContainsValue(TValue value)
		{
			lock (_entries)
				return _entries.ContainsValue(value);
		}

		/// <summary>
		/// Returns value for key, or the default value of the value type
		/// if the key doesn't exist (e.g. null for string).
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public TValue GetValueOrDefault(TKey key)
		{
			TValue result;

			lock (_entries)
				_entries.TryGetValue(key, out result);

			return result;
		}

		/// <summary>
		/// Returns list of key-values that match the predicate.
		/// </summary>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public IEnumerable<KeyValuePair<TKey, TValue>> Get(Func<KeyValuePair<TKey, TValue>, bool> predicate)
		{
			lock (_entries)
				return _entries.Where(predicate);
		}
	}
}
