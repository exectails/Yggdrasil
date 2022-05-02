using System.Collections.Generic;

namespace Yggdrasil.Extensions
{
	/// <summary>
	/// Extensions for the generic Dictionary type.
	/// </summary>
	public static class DictionaryExtensions
	{
		/// <summary>
		/// Returns the value for the given key, or the value's default
		/// value if the key wasn't found.
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="dictionary"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static TValue TryGetValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
		{
			dictionary.TryGetValue(key, out var result);
			return result;
		}

		/// <summary>
		/// Deconstructs a KeyValuePair into a tuple.
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="dictonary"></param>
		/// <param name="key"></param>
		/// <param name="value"></param>
		public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> dictonary, out TKey key, out TValue value)
		{
			key = dictonary.Key;
			value = dictonary.Value;
		}
	}
}
