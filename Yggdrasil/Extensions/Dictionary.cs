// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using System.Collections.Generic;

namespace Yggdrasil.Extensions
{
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
	}
}
