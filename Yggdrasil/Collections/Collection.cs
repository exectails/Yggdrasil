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
		/// <summary>
		/// List of entries.
		/// </summary>
		protected readonly Dictionary<TKey, TValue> _entries = new Dictionary<TKey, TValue>();

		/// <summary>
		/// Returns number of entries in collection.
		/// </summary>
		public int Count { get { lock (_entries) return _entries.Count; } }

		/// <summary>
		/// Adds value to collection.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <exception cref="ArgumentException">
		/// Thrown if the key already exists.
		/// </exception>
		public void Add(TKey key, TValue value)
		{
			lock (_entries)
			{
				if (_entries.ContainsKey(key))
					throw new ArgumentException("The key already exists in the collection.");

				_entries.Add(key, value);
			}
		}

		/// <summary>
		/// Adds value to collection, returns false if key existed already.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		public bool AddIfNotExists(TKey key, TValue value)
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
		public void AddOrReplace(TKey key, TValue value)
		{
			lock (_entries)
				_entries[key] = value;
		}

		/// <summary>
		/// Adds a value to the collection or returns it. Equivalent to
		/// Get and AddOrReplace.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public TValue this[TKey key]
		{
			get => this.Get(key);
			set => this.AddOrReplace(key, value);
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
		public Dictionary<TKey, TValue> Get(Func<KeyValuePair<TKey, TValue>, bool> predicate)
		{
			lock (_entries)
				return _entries.Where(predicate).ToDictionary(a => a.Key, a => a.Value);
		}

		/// <summary>
		/// Returns the value with the given key.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException">
		/// Thrown if the key wasn't found.
		/// </exception>
		public TValue Get(TKey key)
		{
			lock (_entries)
			{
				if (!_entries.TryGetValue(key, out var value))
					throw new ArgumentException("Key not found.");

				return value;
			}
		}

		/// <summary>
		/// Returns the value with the given key via out, returns false
		/// if the value couldn't be found.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool TryGet(TKey key, out TValue value)
		{
			lock (_entries)
				return _entries.TryGetValue(key, out value);
		}

		/// <summary>
		/// Returns a list of all values.
		/// </summary>
		/// <returns></returns>
		public TValue[] GetList()
		{
			lock (_entries)
				return _entries.Values.ToArray();
		}

		/// <summary>
		/// Returns a list of all values that match the given predicate.
		/// </summary>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public TValue[] GetList(Func<TValue, bool> predicate)
		{
			lock (_entries)
				return _entries.Values.Where(predicate).ToArray();
		}

		/// <summary>
		/// Executes the given function an all entries.
		/// </summary>
		/// <param name="func"></param>
		public void Execute(Action<TValue> func)
		{
			lock (_entries)
			{
				foreach (var value in _entries.Values)
					func(value);
			}
		}

		/// <summary>
		/// Returns a list of results queried from all entries.
		/// </summary>
		/// <example>
		/// var characters = world.Regions.Query(a => a.GetCharacters());
		/// </example>
		/// <typeparam name="TObj"></typeparam>
		/// <param name="func"></param>
		/// <returns></returns>
		public TObj[] Query<TObj>(Func<TValue, IEnumerable<TObj>> func)
		{
			lock (_entries)
				return _entries.Values.SelectMany(func).ToArray();
		}
	}
}
