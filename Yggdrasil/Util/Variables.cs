using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Yggdrasil.Util
{
	/// <summary>
	/// Wrapper around a dictionary of objects that can be read from and
	/// serializes to a single string.
	/// </summary>
	/// <example>
	/// var dict = Variables.FromString("SOMEINT:4:1234;SOMESTR:s:test;");
	/// var someInt = dict.GetInt("SOMEINT"); // 1234
	/// 
	/// var dict = new Variables();
	/// dict.SetInt("SOMEOTHERINT", 3456);
	/// var str = dict.ToString(); // SOMEOTHERINT:4:3456;
	/// </example>
	public class Variables
	{
		private static readonly Regex _validityRegex = new Regex("^([^:]+:[^:]+:[^;]*;)+$", RegexOptions.Compiled);
		private static readonly Regex _valueRegex = new Regex("([^:]+):([^:]+):([^;]*);", RegexOptions.Compiled);

		private object _syncLock = new object();
		private Dictionary<string, object> _variables = new Dictionary<string, object>();
		private string _cache = null;

		/// <summary>
		/// Creates new instance.
		/// </summary>
		public Variables()
		{
		}

		/// <summary>
		/// Creates new instance from the string of values.
		/// </summary>
		/// <param name="values"></param>
		/// <returns></returns>
		public static Variables FromString(string values)
		{
			if (!_validityRegex.IsMatch(values))
				throw new ArgumentException("Invalid string format.");

			var dict = new Variables();
			dict.Parse(values);

			return dict;
		}

		/// <summary>
		/// Returns the value for the variable with the given name,
		/// or the default if the variable doesn't exist.
		/// </summary>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="name"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public TValue Get<TValue>(string name, TValue def = default(TValue))
		{
			object result;
			lock (_syncLock)
			{
				if (!_variables.TryGetValue(name, out result))
					return def;
			}

			if (!(result is TValue value))
				throw new InvalidCastException($"Variable '{name}' is not of type '{typeof(TValue)}', but '{result.GetType()}'.");

			return value;
		}

		/// <summary>
		/// Returns variable with given name as object.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public object Get(string name)
		{
			object result;
			lock (_syncLock)
			{
				if (!_variables.TryGetValue(name, out result))
					return null;
			}

			return result;
		}

		/// <summary>
		/// Returns the given variable or the default value if it doesn't
		/// exist.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public byte GetByte(string name, byte def = 0)
		{
			return this.Get(name, def);
		}

		/// <summary>
		/// Returns the given variable or the default value if it doesn't
		/// exist.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public short GetShort(string name, short def = 0)
		{
			return this.Get(name, def);
		}

		/// <summary>
		/// Returns the given variable or the default value if it doesn't
		/// exist.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public int GetInt(string name, int def = 0)
		{
			return this.Get(name, def);
		}

		/// <summary>
		/// Returns the given variable or the default value if it doesn't
		/// exist.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public long GetLong(string name, long def = 0)
		{
			return this.Get(name, def);
		}

		/// <summary>
		/// Returns the given variable or the default value if it doesn't
		/// exist.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public float GetFloat(string name, float def = 0)
		{
			return this.Get(name, def);
		}

		/// <summary>
		/// Returns the given variable or the default value if it doesn't
		/// exist.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public string GetString(string name, string def = null)
		{
			return this.Get(name, def);
		}

		/// <summary>
		/// Returns the given variable or the default value if it doesn't
		/// exist.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public byte[] GetBytes(string name, byte[] def = null)
		{
			return this.Get(name, def);
		}

		/// <summary>
		/// Returns the given variable or the default value if it doesn't
		/// exist.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public bool GetBool(string name, bool def = false)
		{
			return this.Get(name, def);
		}

		/// <summary>
		/// Sets the value for the given name. Removes variable if value
		/// is null.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public void Set(string name, object value)
		{
			if (value == null)
			{
				this.Remove(name);
				return;
			}

			lock (_syncLock)
			{
				_variables[name] = value;
				_cache = null;
			}
		}

		/// <summary>
		/// Sets the given variable.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public void SetByte(string name, byte value)
		{
			this.Set(name, value);
		}

		/// <summary>
		/// Sets the given variable.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public void SetShort(string name, short value)
		{
			this.Set(name, value);
		}

		/// <summary>
		/// Sets the given variable.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public void SetInt(string name, int value)
		{
			this.Set(name, value);
		}

		/// <summary>
		/// Sets the given variable.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public void SetLong(string name, long value)
		{
			this.Set(name, value);
		}

		/// <summary>
		/// Sets the given variable.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public void SetFloat(string name, float value)
		{
			this.Set(name, value);
		}

		/// <summary>
		/// Sets the given variable.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public void SetString(string name, string value)
		{
			this.Set(name, value);
		}

		/// <summary>
		/// Sets the given variable.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public void SetBytes(string name, byte[] value)
		{
			this.Set(name, value);
		}

		/// <summary>
		/// Sets the given variable.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public void SetBool(string name, bool value)
		{
			this.Set(name, value);
		}

		/// <summary>
		/// Removes the variable with the given name if it exists.
		/// </summary>
		/// <param name="name"></param>
		public void Remove(string name)
		{
			lock (_syncLock)
			{
				_variables.Remove(name);
				_cache = null;
			}
		}

		/// <summary>
		/// Returns list of all variables.
		/// </summary>
		/// <returns></returns>
		public IDictionary<string, object> GetList()
		{
			lock (_syncLock)
				return new Dictionary<string, object>(_variables);
		}

		/// <summary>
		/// Loads all given variables.
		/// </summary>
		/// <param name="variables"></param>
		public void Load(IDictionary<string, object> variables)
		{
			lock (_syncLock)
			{
				foreach (var variable in variables)
					_variables[variable.Key] = variable.Value;
			}
		}

		/// <summary>
		/// Returns string type identifier for the value, returns null
		/// if the type is not supported.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		private static string GetTypeIdent(object value)
		{
			var typeCode = Type.GetTypeCode(value.GetType());

			switch (typeCode)
			{
				case TypeCode.Byte:
				case TypeCode.SByte: return "1";
				case TypeCode.Int16:
				case TypeCode.UInt16: return "2";
				case TypeCode.Int32:
				case TypeCode.UInt32: return "4";
				case TypeCode.Int64:
				case TypeCode.UInt64: return "8";
				case TypeCode.Single: return "f";
				case TypeCode.String: return "s";
				case TypeCode.Boolean: return "b";

				case TypeCode.Object:
					if (value is byte[])
						return "B";
					break;
			}

			return null;
		}

		/// <summary>
		/// Returns dictionary in the format "key:varType:value;...".
		/// </summary>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">
		/// Thrown if instance contains a variable that can't be represented
		/// as a string.
		/// </exception>
		public override string ToString()
		{
			var sb = new StringBuilder();

			lock (_syncLock)
			{
				if (_variables.Count < 1)
					return string.Empty;

				if (_cache != null)
					return _cache;

				foreach (var variable in _variables)
				{
					var sType = GetTypeIdent(variable.Value);
					if (sType == null)
						throw new ArgumentException($"Unsupported type '{variable.Value.GetType().Name}' on '{variable.Key}'.");

					if (sType == "b")
						sb.AppendFormat("{0}:{1}:{2};", variable.Key, sType, (bool)variable.Value ? "1" : "0");
					else if (sType == "s")
						sb.AppendFormat("{0}:{1}:{2};", variable.Key, sType, ((string)variable.Value).Replace(";", "%S").Replace(":", "%C"));
					else if (sType == "f")
						sb.AppendFormat("{0}:{1}:{2};", variable.Key, sType, ((float)variable.Value).ToString(CultureInfo.InvariantCulture));
					else if (sType == "B")
						sb.AppendFormat("{0}:{1}:{2};", variable.Key, sType, Convert.ToBase64String((byte[])variable.Value));
					else
						sb.AppendFormat("{0}:{1}:{2};", variable.Key, sType, variable.Value);
				}

				return (_cache = sb.ToString());
			}
		}

		/// <summary>
		/// Reads a string in the format "key:varType:value;..." and adds
		/// the values to this tag collection.
		/// </summary>
		/// <param name="str"></param>
		public void Parse(string str)
		{
			if (str == null)
				throw new ArgumentNullException(nameof(str));

			str = str.Trim();
			if (str == "")
				return;

			foreach (Match match in _valueRegex.Matches(str))
			{
				var name = match.Groups[1].Value;
				var type = match.Groups[2].Value;
				var value = match.Groups[3].Value;

				switch (type)
				{
					case "1": this.Set(name, Convert.ToByte(value)); break;
					case "2": this.Set(name, Convert.ToInt16(value)); break;
					case "4": this.Set(name, Convert.ToInt32(value)); break;
					case "8": this.Set(name, Convert.ToInt64(value)); break;
					case "f": this.Set(name, Convert.ToSingle(value, CultureInfo.InvariantCulture)); break;
					case "s": this.Set(name, value.Replace("%S", ";").Replace("%C", ":")); break;
					case "b": this.Set(name, value == "1"); break;
					case "B": this.Set(name, Convert.FromBase64String(value)); break;
				}
			}
		}
	}
}
