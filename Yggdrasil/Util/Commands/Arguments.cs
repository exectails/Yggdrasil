using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Yggdrasil.Extensions;

namespace Yggdrasil.Util.Commands
{
	/// <summary>
	/// Represents arguments in a command.
	/// </summary>
	public class Arguments
	{
		private static readonly Regex TokenRegex = new Regex(@"((?:(?<key>[a-z0-9_]+)\:)?(?<val>(?:""[^""]+(?:""|$))|(?:[^ ]+)))", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		private readonly List<string> _indexed = new List<string>();
		private readonly Dictionary<string, string> _named = new Dictionary<string, string>();

		/// <summary>
		/// Returns the amount of all parameters.
		/// </summary>
		public int Count => _indexed.Count + _named.Count;

		/// <summary>
		/// Returns the amount of indexed parameters.
		/// </summary>
		public int IndexedCount => _indexed.Count;

		/// <summary>
		/// Returns the amount of named parameters.
		/// </summary>
		public int NamedCount => _named.Count;

		/// <summary>
		/// Creates new Arguments instance.
		/// </summary>
		public Arguments()
		{
		}

		/// <summary>
		/// Creates new Arguments instance and parses the given line.
		/// </summary>
		public Arguments(string line)
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
		/// <param name="name"></param>
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
			foreach (Match match in TokenRegex.Matches(line))
			{
				var key = match.Groups["key"].Value.Trim();
				var val = match.Groups["val"].Value.Trim('"').Trim();

				if (val.IsNullOrWhiteSpace())
					continue;

				if (key.IsNullOrWhiteSpace())
					_indexed.Add(val);
				else
					_named.Add(key, val);
			}

			// TODO: Add a proper tokenizer
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
		/// Returns the argument with the given name. Returns given default
		/// if parameter wasn't found.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public string Get(string name, string defaultValue = null)
		{
			if (!this.Contains(name))
				return defaultValue;

			return _named[name];
		}

		/// <summary>
		/// Returns the argument with the given name via out. Returns false
		/// if parameter wasn't found.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool TryGet(string name, out string value)
		{
			value = null;

			if (!this.Contains(name))
				return false;

			value = _named[name];
			return true;
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
