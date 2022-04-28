using System;
using System.Collections.Generic;

namespace Yggdrasil.Util
{
	/// <summary>
	/// Wrapper around a dictionary of objects that can be read from and
	/// serializes to a single string.
	/// </summary>
	public class Variables
	{
		private readonly object _syncLock = new object();
		private Dictionary<string, object> _variables = new Dictionary<string, object>();
		private string _cache = null;

		/// <summary>
		/// Gets or sets this instance's cached variable string.
		/// </summary>
		protected string Cache
		{
			get { lock (_syncLock) return _cache; }
			set { lock (_syncLock) _cache = value; }
		}

		/// <summary>
		/// Returns the number of variables in this instance.
		/// </summary>
		public int Count
		{
			get { lock (_syncLock) return _variables.Count; }
		}

		/// <summary>
		/// Creates new instance.
		/// </summary>
		public Variables()
		{
		}

		/// <summary>
		/// Returns true if a variable with the given name exists.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public bool Has(string name)
		{
			lock (_syncLock)
				return _variables.ContainsKey(name);
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
			if (!this.TryGet(name, out TValue value))
				return def;

			return value;
		}

		/// <summary>
		/// Returns variable with given name as object, returns null
		/// if the variables doesn't exist.
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
		/// Returns the value with the given name via out, returns false
		/// if the variable wasn't found.
		/// </summary>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool TryGet<TValue>(string name, out TValue value)
		{
			object result;
			lock (_syncLock)
			{
				if (!_variables.TryGetValue(name, out result))
				{
					value = default(TValue);
					return false;
				}
			}

			if (!(result is TValue))
				throw new InvalidCastException($"Variable '{name}' is not of type '{typeof(TValue)}', but '{result.GetType()}'.");

			value = (TValue)result;
			return true;
		}

		/// <summary>
		/// Returns the given variable or the default value if it doesn't
		/// exist.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public byte GetByte(string name, byte def = 0)
			=> this.Get(name, def);

		/// <summary>
		/// Returns the value of the variable with the given name via out,
		/// returns false if the variable doesn't exist.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool TryGetByte(string name, out byte value)
			=> this.TryGet(name, out value);

		/// <summary>
		/// Returns the given variable or the default value if it doesn't
		/// exist.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public sbyte GetSByte(string name, sbyte def = 0)
			=> (sbyte)this.GetByte(name, (byte)def);

		/// <summary>
		/// Returns the value of the variable with the given name via out,
		/// returns false if the variable doesn't exist.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool TryGetSByte(string name, out sbyte value)
			=> this.TryGet(name, out value);

		/// <summary>
		/// Returns the given variable or the default value if it doesn't
		/// exist.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public short GetShort(string name, short def = 0)
			=> this.Get(name, def);

		/// <summary>
		/// Returns the value of the variable with the given name via out,
		/// returns false if the variable doesn't exist.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool TryGetShort(string name, out short value)
			=> this.TryGet(name, out value);

		/// <summary>
		/// Returns the given variable or the default value if it doesn't
		/// exist.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public ushort GetUShort(string name, ushort def = 0)
			=> (ushort)this.GetShort(name, (short)def);

		/// <summary>
		/// Returns the value of the variable with the given name via out,
		/// returns false if the variable doesn't exist.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool TryGetUShort(string name, out ushort value)
			=> this.TryGet(name, out value);

		/// <summary>
		/// Returns the given variable or the default value if it doesn't
		/// exist.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public int GetInt(string name, int def = 0)
			=> this.Get(name, def);

		/// <summary>
		/// Returns the value of the variable with the given name via out,
		/// returns false if the variable doesn't exist.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool TryGetInt(string name, out int value)
			=> this.TryGet(name, out value);

		/// <summary>
		/// Returns the given variable or the default value if it doesn't
		/// exist.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public uint GetUInt(string name, uint def = 0)
			=> (uint)this.GetInt(name, (int)def);

		/// <summary>
		/// Returns the value of the variable with the given name via out,
		/// returns false if the variable doesn't exist.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool TryGetUInt(string name, out uint value)
			=> this.TryGet(name, out value);

		/// <summary>
		/// Returns the given variable or the default value if it doesn't
		/// exist.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public long GetLong(string name, long def = 0)
			=> this.Get(name, def);

		/// <summary>
		/// Returns the value of the variable with the given name via out,
		/// returns false if the variable doesn't exist.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool TryGetLong(string name, out long value)
			=> this.TryGet(name, out value);

		/// <summary>
		/// Returns the given variable or the default value if it doesn't
		/// exist.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public ulong GetULong(string name, ulong def = 0)
			=> (ulong)this.GetLong(name, (long)def);

		/// <summary>
		/// Returns the value of the variable with the given name via out,
		/// returns false if the variable doesn't exist.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool TryGetULong(string name, out ulong value)
			=> this.TryGet(name, out value);

		/// <summary>
		/// Returns the given variable or the default value if it doesn't
		/// exist.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public float GetFloat(string name, float def = 0)
			=> this.Get(name, def);

		/// <summary>
		/// Returns the value of the variable with the given name via out,
		/// returns false if the variable doesn't exist.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool TryGetFloat(string name, out float value)
			=> this.TryGet(name, out value);

		/// <summary>
		/// Returns the given variable or the default value if it doesn't
		/// exist.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public string GetString(string name, string def = null)
			=> this.Get(name, def);

		/// <summary>
		/// Returns the value of the variable with the given name via out,
		/// returns false if the variable doesn't exist.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool TryGetString(string name, out string value)
			=> this.TryGet(name, out value);

		/// <summary>
		/// Returns the given variable or the default value if it doesn't
		/// exist.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public byte[] GetBytes(string name, byte[] def = null)
			=> this.Get(name, def);

		/// <summary>
		/// Returns the value of the variable with the given name via out,
		/// returns false if the variable doesn't exist.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool TryGetBytes(string name, out byte[] value)
			=> this.TryGet(name, out value);

		/// <summary>
		/// Returns the given variable or the default value if it doesn't
		/// exist.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public bool GetBool(string name, bool def = false)
			=> this.Get(name, def);

		/// <summary>
		/// Returns the value of the variable with the given name via out,
		/// returns false if the variable doesn't exist.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool TryGetBool(string name, out bool value)
			=> this.TryGet(name, out value);

		/// <summary>
		/// Sets the value for the given name. Removes variable if value
		/// is null.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public object Set(string name, object value)
		{
			if (value == null)
			{
				this.Remove(name);
			}
			else
			{
			lock (_syncLock)
			{
				_variables[name] = value;
				_cache = null;
			}
		}

			return null;
		}

		/// <summary>
		/// Sets the given variable.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public byte SetByte(string name, byte value)
		{
			this.Set(name, value);
			return value;
		}

		/// <summary>
		/// Sets the given variable.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public sbyte SetSByte(string name, sbyte value) => (sbyte)this.SetByte(name, (byte)value);

		/// <summary>
		/// Sets the given variable.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public short SetShort(string name, short value)
		{
			this.Set(name, value);
			return value;
		}

		/// <summary>
		/// Sets the given variable.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public ushort SetUShort(string name, ushort value) => (ushort)this.SetShort(name, (short)value);

		/// <summary>
		/// Sets the given variable.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public int SetInt(string name, int value)
		{
			this.Set(name, value);
			return value;
		}

		/// <summary>
		/// Sets the given variable.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public uint SetUInt(string name, uint value) => (uint)this.SetInt(name, (int)value);

		/// <summary>
		/// Sets the given variable.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public long SetLong(string name, long value)
		{
			this.Set(name, value);
			return value;
		}

		/// <summary>
		/// Sets the given variable.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public ulong SetULong(string name, ulong value) => (ulong)this.SetLong(name, (long)value);

		/// <summary>
		/// Sets the given variable.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public float SetFloat(string name, float value)
		{
			this.Set(name, value);
			return value;
		}

		/// <summary>
		/// Sets the given variable.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public string SetString(string name, string value)
		{
			this.Set(name, value);
			return value;
		}

		/// <summary>
		/// Sets the given variable.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public byte[] SetBytes(string name, byte[] value)
		{
			this.Set(name, value);
			return value;
		}

		/// <summary>
		/// Sets the given variable.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public bool SetBool(string name, bool value)
		{
			this.Set(name, value);
			return value;
		}

		/// <summary>
		/// Toggles boolean value on or off. If the variable wasn't set
		/// yet, it becomes true.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public bool ToggleBool(string name)
		{
			var value = !this.GetBool(name, false);
			return this.SetBool(name, value);
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
	}
}
