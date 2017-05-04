// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;

namespace Yggdrasil.Util.Commands
{
	/// <summary>
	/// Represents arguments in a command.
	/// </summary>
	public class Arguments
	{
		private List<string> _indexed;
		private Dictionary<string, string> _named;

		/// <summary>
		/// Returns the amount of all parameters.
		/// </summary>
		public int Count { get { return _indexed.Count + _named.Count; } }

		/// <summary>
		/// Creates new Arguments instance.
		/// </summary>
		public Arguments()
		{
			_indexed = new List<string>();
			_named = new Dictionary<string, string>();
		}

		/// <summary>
		/// Creates new Arguments instance and parses the given line.
		/// </summary>
		public Arguments(string line)
			: this()
		{
			this.Parse(line);
		}

		/// <summary>
		/// Returns true if an argument with the given index exists.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public bool Contains(int index)
		{
			return (index >= 0 && index < _indexed.Count);
		}

		/// <summary>
		/// Returns true if an argument with the given name exists.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public bool Contains(string name)
		{
			return _named.ContainsKey(name);
		}

		/// <summary>
		/// Parses given line into this Arguments instance.
		/// </summary>
		/// <param name="line"></param>
		public void Parse(string line)
		{
			var quote = false;

			for (int i = 0, n = 0; i <= line.Length; ++i)
			{
				if ((i == line.Length || line[i] == ' ') && !quote)
				{
					if (i - n > 0)
					{
						var str = line.Substring(n, i - n);
						var index = str.IndexOf(":");
						if (index != -1)
						{
							var key = str.Substring(0, index).Trim(' ', '"');
							var val = str.Substring(index + 1, str.Length - index - 1).Trim(' ', '"');

							_named.Add(key, val);
						}
						else
							_indexed.Add(str.Trim(' ', '"'));
					}

					n = i + 1;
					continue;
				}

				if (line[i] == '"')
					quote = !quote;
			}
		}

		/// <summary>
		/// Returns the argument at the given index.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException">
		/// Thrown if index doesn't exist.
		/// </exception>
		public string Get(int index)
		{
			if (!this.Contains(index))
				throw new ArgumentException("Index '" + index + "' doesn't exist.");

			return _indexed[index];
		}

		/// <summary>
		/// Returns the argument with the given name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException">
		/// Thrown if no argument with the given name exists.
		/// </exception>
		public string Get(string name)
		{
			if (!this.Contains(name))
				throw new ArgumentException("Argument with name '" + name + "' doesn't exist.");

			return _named[name];
		}

		/// <summary>
		/// Returns the argument with the given name. Returns given default
		/// if parameter wasn't found.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public string Get(string name, string defaultValue)
		{
			if (!this.Contains(name))
				return defaultValue;

			return _named[name];
		}

		/// <summary>
		/// Returns all arguments, starting with the indexed ones in order,
		/// followed by the named ones, with no order guaranteed.
		/// </summary>
		/// <returns></returns>
		public List<string> GetAll()
		{
			var result = new List<string>();

			result.AddRange(_indexed);
			foreach (var kv in _named)
			{
				if (kv.Key.Contains(" ") || kv.Value.Contains(" "))
					result.Add('"' + kv.Key + ":" + kv.Value + '"');
				else
					result.Add(kv.Key + ":" + kv.Value);
			}

			return result;
		}

		/// <summary>
		/// Removes the argument at the given index.
		/// </summary>
		/// <param name="index"></param>
		/// <exception cref="ArgumentException">
		/// Thrown if index doesn't exist.
		/// </exception>
		public void Remove(int index)
		{
			if (!this.Contains(index))
				throw new ArgumentException("Index '" + index + "' doesn't exist.");

			_indexed.RemoveAt(index);
		}

		/// <summary>
		/// Removes the argument with the given name.
		/// </summary>
		/// <param name="name"></param>
		/// <exception cref="ArgumentException">
		/// Thrown if argument doesn't exist.
		/// </exception>
		public void Remove(string name)
		{
			if (!this.Contains(name))
				throw new ArgumentException("Argument with name '" + name + "' doesn't exist.");

			_named.Remove(name);
		}
	}
}
