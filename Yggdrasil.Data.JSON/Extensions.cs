using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Yggdrasil.Data.JSON
{
	/// <summary>
	/// Extensions for the JObject type.
	/// </summary>
	public static class JsonExtensions
	{
		/// <summary>
		/// Reads value and returns it, or default if it was missing.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="key"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public static bool ReadBool(this JObject obj, string key, bool def = false) { return (bool)(obj[key] ?? def); }

		/// <summary>
		/// Reads value and returns it, or default if it was missing.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="key"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public static byte ReadByte(this JObject obj, string key, byte def = 0) { return (byte)(obj[key] ?? def); }

		/// <summary>
		/// Reads value and returns it, or default if it was missing.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="key"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public static sbyte ReadSByte(this JObject obj, string key, sbyte def = 0) { return (sbyte)(obj[key] ?? def); }

		/// <summary>
		/// Reads value and returns it, or default if it was missing.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="key"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public static short ReadShort(this JObject obj, string key, short def = 0) { return (short)(obj[key] ?? def); }

		/// <summary>
		/// Reads value and returns it, or default if it was missing.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="key"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public static ushort ReadUShort(this JObject obj, string key, ushort def = 0) { return (ushort)(obj[key] ?? def); }

		/// <summary>
		/// Reads value and returns it, or default if it was missing.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="key"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public static int ReadInt(this JObject obj, string key, int def = 0) { return (int)(obj[key] ?? def); }

		/// <summary>
		/// Reads value and returns it, or default if it was missing.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="key"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public static uint ReadUInt(this JObject obj, string key, uint def = 0) { return (uint)(obj[key] ?? def); }

		/// <summary>
		/// Reads value and returns it, or default if it was missing.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="key"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public static float ReadFloat(this JObject obj, string key, float def = 0) { return (float)(obj[key] ?? def); }

		/// <summary>
		/// Reads value and returns it, or default if it was missing.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="key"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public static double ReadDouble(this JObject obj, string key, double def = 0) { return (double)(obj[key] ?? def); }

		/// <summary>
		/// Reads value and returns it, or default if it was missing.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="key"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public static string ReadString(this JObject obj, string key, string def = "") { return (string)(obj[key] ?? def); }

		/// <summary>
		/// Reads value as string and parses it into enum type.
		/// Returns the default if key wasn't found.
		/// </summary>
		/// <remarks>
		/// Parses Flags if the values are separated by commas ","
		/// or pipes "|".
		/// </remarks>
		/// <param name="obj"></param>
		/// <param name="key"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public static TEnum ReadEnum<TEnum>(this JObject obj, string key, TEnum def = default(TEnum))
		{
			if (!obj.ContainsKey(key))
				return def;

			var str = obj.ReadString(key);
			if (str.Contains("|"))
				str = str.Replace("|", ",");

			return (TEnum)Enum.Parse(typeof(TEnum), str);
		}

		/// <summary>
		/// Reads and returns single value array.
		/// </summary>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="obj"></param>
		/// <param name="key"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public static TValue[] ReadArray<TValue>(this JObject obj, string key, TValue[] def = default(TValue[]))
		{
			if (!obj.ContainsKey(key))
				return def;

			return obj[key].Select(a => a.ToObject<TValue>()).ToArray();
		}

		/// <summary>
		/// Returns true if object contains all keys.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="keys"></param>
		/// <returns></returns>
		public static bool ContainsKeys(this JObject obj, params string[] keys)
		{
			if (keys.Length == 1)
				return (obj[keys[0]] != null);

			return keys.All(key => obj[key] != null);
		}

		/// <summary>
		/// Returns true if object containes key.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static bool ContainsKey(this JObject obj, string key)
		{
			return (obj[key] != null);
		}

		/// <summary>
		/// Throws exception if one of the keys is missing from the object.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="keys"></param>
		/// <exception cref="MandatoryValueException"></exception>
		public static void AssertNotMissing(this JObject obj, params string[] keys)
		{
			foreach (var key in keys)
			{
				if (!obj.ContainsKey(key))
					throw new MandatoryValueException(null, key, obj);
			}
		}

		/// <summary>
		/// Provides an iterator over all objects in the list with the given
		/// name. If the property doesn't exists no elements are returned.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static IEnumerable<JObject> ForEachObject(this JObject obj, string key)
		{
			if (!obj.ContainsKey(key))
				yield break;

			foreach (JObject element in obj[key])
				yield return element;
		}

		/// <summary>
		/// Provides an iterator over all elements in the list with the
		/// given name. If the property doesn't exists no elements are
		/// returned.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static IEnumerable<TType> ForEach<TType>(this JObject obj, string key)
		{
			if (!obj.ContainsKey(key))
				yield break;

			foreach (JValue element in obj[key])
				yield return (TType)element.Value;
		}
	}
}
