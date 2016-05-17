// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using System;
using System.Collections.Generic;
using Yggdrasil.Util;

namespace Yggdrasil.Extensions
{
	public static class RandomExtensions
	{
		/// <summary>
		/// Returns a random item from the given list.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list">The list.</param>
		/// <returns></returns>
		public static T Random<T>(this IList<T> list)
		{
			return list[RandomProvider.Get().Next(list.Count)];
		}

		/// <summary>
		/// Returns a random number between min and max (incl).
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="rnd"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public static float Between(this Random rnd, float min, float max)
		{
			return (float)(min + (rnd.NextDouble() * (max - min)));
		}

		/// <summary>
		/// Returns a random value from the given ones.
		/// </summary>
		/// <param name="values"></param>
		public static T Rnd<T>(this Random rnd, params T[] values)
		{
			if (values == null || values.Length == 0)
				throw new ArgumentException("Values may not be null or empty.");

			return values[rnd.Next(values.Length)];
		}

		/// <summary>
		/// Returns random long.
		/// </summary>
		/// <param name="rnd"></param>
		/// <returns></returns>
		public static long NextInt64(this Random rnd)
		{
			return (((long)rnd.Next() << 8 * 4 - 1) + rnd.Next());
		}
	}
}
