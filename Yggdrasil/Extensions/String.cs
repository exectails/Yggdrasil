using System;

namespace Yggdrasil.Extensions
{
	/// <summary>
	/// Extensions for the string type.
	/// </summary>
	public static class StringExtensions
	{
		/// <summary>
		/// Calculates differences between 2 strings.
		/// </summary>
		/// <remarks>
		/// http://en.wikipedia.org/wiki/Levenshtein_distance
		/// </remarks>
		/// <example>
		/// <code>
		/// "test".GetLevenshteinDistance("test")       // == 0
		/// "test1".GetLevenshteinDistance("test2")     // == 1
		/// "test1".GetLevenshteinDistance("test1 asd") // == 4
		/// </code>
		/// </example>
		public static int GetLevenshteinDistance(this string str, string compare, bool caseSensitive = true)
		{
			if (!caseSensitive)
			{
				str = str.ToLower();
				compare = compare.ToLower();
			}

			var sLen = str.Length;
			var cLen = compare.Length;
			var result = new int[sLen + 1, cLen + 1];

			if (sLen == 0)
				return cLen;

			if (cLen == 0)
				return sLen;

			for (var i = 0; i <= sLen; result[i, 0] = i++) ;
			for (var i = 0; i <= cLen; result[0, i] = i++) ;

			for (var i = 1; i <= sLen; i++)
			{
				for (var j = 1; j <= cLen; j++)
				{
					var cost = (compare[j - 1] == str[i - 1]) ? 0 : 1;
					result[i, j] = Math.Min(Math.Min(result[i - 1, j] + 1, result[i, j - 1] + 1), result[i - 1, j - 1] + cost);
				}
			}

			return result[sLen, cLen];
		}

		/// <summary>
		/// Indicates whether a specified string is null, empty, or consists
		/// only of white-space characters.
		/// </summary>
		/// <param name="value">The string to test.</param>
		/// <returns></returns>
		public static bool IsNullOrWhiteSpace(this string value)
		{
			if (value == null)
				return true;

			for (var i = 0; i < value.Length; ++i)
			{
				if (!char.IsWhiteSpace(value[i]))
					return false;
			}

			return true;
		}
	}
}
