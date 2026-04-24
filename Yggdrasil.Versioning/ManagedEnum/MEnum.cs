using System;
using System.Collections.Generic;
using System.IO;
using Yggdrasil.Versioning.IO;

namespace Yggdrasil.Versioning.ManagedEnum
{
	/// <summary>
	/// A managed enum mapper that allows for dynamic mapping of enum keys
	/// to integer values, with support for insertion and shifting of
	/// values.
	/// </summary>
	public class MEnum<TEnum> where TEnum : struct, Enum
	{
		/// <summary>
		/// Provides a shared instance of the mapper for the specified
		/// type.
		/// </summary>
		public static readonly MEnum<TEnum> Shared = new MEnum<TEnum>();

		private readonly Dictionary<TEnum, int> _lookupTable = new Dictionary<TEnum, int>();
		private readonly Dictionary<int, TEnum> _keyTable = new Dictionary<int, TEnum>();
		private readonly List<TEnum> _insertList = new List<TEnum>();
		private readonly HashSet<TEnum> _updatedKeys = new HashSet<TEnum>();
		private int _nextInsertValue = 0;

		/// <summary>
		/// Returns true if a value exists for the specified enum key.
		/// </summary>
		/// <param name="enumKey"></param>
		/// <returns></returns>
		public bool HasValue(TEnum enumKey)
			=> _lookupTable.ContainsKey(enumKey);

		/// <summary>
		/// Returns the value associated with the specified enum key.
		/// </summary>
		/// <param name="enumKey"></param>
		/// <returns></returns>
		public int GetValue(TEnum enumKey)
		{
			if (!_lookupTable.TryGetValue(enumKey, out var value))
				throw new ArgumentException($"Value {enumKey} not found.", nameof(enumKey));

			return value;
		}

		/// <summary>
		/// Returns the value associated with the specified enum key, or
		/// a default value if the enum key was not found.
		/// </summary>
		/// <param name="enumKey"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public int GetValue(TEnum enumKey, int defaultValue)
		{
			if (!_lookupTable.TryGetValue(enumKey, out var value))
				return defaultValue;

			return value;
		}

		/// <summary>
		/// Returns the value associated with the specified enum key via
		/// out. Returns false if the enum key was not found.
		/// </summary>
		/// <param name="enumKey"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool TryGetValue(TEnum enumKey, out int value)
			=> _lookupTable.TryGetValue(enumKey, out value);

		/// <summary>
		/// Sets the value associated with the specified enum key.
		/// </summary>
		/// <remarks>
		/// Does not shift values.
		/// </remarks>
		/// <param name="enumKey"></param>
		/// <param name="value"></param>
		public void SetValue(TEnum enumKey, int value)
		{
			_lookupTable[enumKey] = value;
			this.UpdateNewValue();
		}

		/// <summary>
		/// Adds a new value for the specified enum key at the end of
		/// the list.
		/// </summary>
		/// <param name="enumKey"></param>
		public void InsertValue(TEnum enumKey)
			=> this.InsertValue(enumKey, _nextInsertValue);

		/// <summary>
		/// Adds the value associated with the specified enum key,
		/// potentially shifting aready existing values to maintain
		/// sequential order.
		/// </summary>
		/// <param name="enumKey"></param>
		/// <param name="value"></param>
		public void InsertValue(TEnum enumKey, int value)
		{
			if (_lookupTable.TryGetValue(enumKey, out var existingValue))
			{
				var startIndex = _insertList.IndexOf(enumKey);
				var oldValue = existingValue;
				var newValue = value;

				_lookupTable[enumKey] = newValue;

				if (startIndex != -1 && startIndex + 1 < _insertList.Count)
				{
					var nextEnum = _insertList[startIndex + 1];
					var nextValue = _lookupTable[nextEnum];

					if (newValue >= nextValue)
					{
						var sequentialValue = oldValue + 1;
						if (newValue > nextValue)
							sequentialValue = nextValue;

						var currentNewValue = newValue + 1;

						for (var i = startIndex + 1; i < _insertList.Count; i++)
						{
							var key = _insertList[i];
							var val = _lookupTable[key];

							if (val != sequentialValue)
								break;

							_lookupTable[key] = currentNewValue;

							currentNewValue++;
							sequentialValue++;
						}
					}
				}
			}
			else
			{
				var insertAtEnd = value >= _nextInsertValue;
				if (!insertAtEnd)
				{
					var updated = _updatedKeys;
					updated.Clear();

					var shiftValue = value;

					for (var v = value; v <= _nextInsertValue; v++)
					{
						var found = false;

						foreach (var kv in _lookupTable)
						{
							if (updated.Contains(kv.Key))
								continue;

							if (kv.Value == v)
							{
								_lookupTable[kv.Key] = ++shiftValue;

								updated.Add(kv.Key);
								found = true;
								break;
							}
						}

						if (!found)
							break;
					}
				}

				_lookupTable[enumKey] = value;
				_insertList.Add(enumKey);
			}

			this.UpdateNewValue();
		}

		/// <summary>
		/// Updates the next new value for insertion.
		/// </summary>
		private void UpdateNewValue()
		{
			foreach (var tableValue in _lookupTable.Values)
			{
				if (tableValue >= _nextInsertValue)
					_nextInsertValue = tableValue + 1;
			}

			_keyTable.Clear();
			foreach (var kv in _lookupTable)
				_keyTable[kv.Value] = kv.Key;
		}

		/// <summary>
		/// Returns the enum key associated with the specified value.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public TEnum GetKey(int value)
		{
			if (!_keyTable.TryGetValue(value, out var key))
				throw new ArgumentException($"Value {value} not found.", nameof(value));

			return key;
		}

		/// <summary>
		/// Returns the enum key associated with the specified value via
		/// out. Returns false if the value was not found.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool TryGetKey(int value, out TEnum key)
			=> _keyTable.TryGetValue(value, out key);

		/// <summary>
		/// Resets the enum mapping, removing all values.
		/// </summary>
		public void ClearValues()
		{
			_lookupTable.Clear();
			_keyTable.Clear();
			_insertList.Clear();
			_nextInsertValue = 0;
		}

		/// <summary>
		/// Loads enum mappings from a file.
		/// </summary>
		/// <remarks>
		/// Expects the file to have lines in the format "EnumKey" or
		/// "EnumKey=Value", where EnumKey is the name of the enum member
		/// and Value is the associated integer value.
		///
		/// Ignores empty lines and whitespaces. Supports comments
		/// starting with '#' or '//'.
		/// </remarks>
		/// <param name="filePath"></param>
		public void LoadFile(string filePath)
		{
			var preprocessor = new Preprocessor();

			foreach (var line in preprocessor.ProcessLines(filePath))
			{
				var trimmedLine = line.Trim();

				var emptyOrComment = string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#") || trimmedLine.StartsWith("//");
				if (emptyOrComment)
					continue;

				var parts = line.Split('=');

				if (parts.Length == 1)
				{
					var enumKey = (TEnum)Enum.Parse(typeof(TEnum), parts[0].Trim());

					this.InsertValue(enumKey);
				}
				else if (parts.Length == 2)
				{
					var enumKey = (TEnum)Enum.Parse(typeof(TEnum), parts[0].Trim());
					var value = int.Parse(parts[1].Trim());

					this.InsertValue(enumKey, value);
				}
				else
				{
					throw new FileLoadException($"Invalid line format: '{line}'. Expected 'EnumKey=Value'.");
				}
			}
		}

		/// <summary>
		/// Exception for errors that occur during file loading of enum
		/// mappings.
		/// </summary>
		public class FileLoadException : Exception
		{
			/// <summary>
			/// Creates new instance.
			/// </summary>
			/// <param name="message"></param>
			public FileLoadException(string message) : base(message)
			{
			}
		}
	}
}
