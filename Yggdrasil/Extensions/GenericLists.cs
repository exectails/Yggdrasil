// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using System.Collections.Generic;
using System.Linq;
using Yggdrasil.Util;

namespace Yggdrasil.Extensions
{
	/// <summary>
	/// Extensions for lists.
	/// </summary>
	public static class GenericListExtensions
	{
		/// <summary>
		/// Returns a random item from the given list.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list">The list.</param>
		/// <returns></returns>
		public static T Random<T>(this IList<T> list)
		{
			var rnd = RandomProvider.Get();
			var max = list.Count;
			var i = rnd.Next(max);

			return list[i];
		}

		/// <summary>
		/// Returns a random item from the given list.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list">The list.</param>
		/// <returns></returns>
		public static T Random<T>(this IEnumerable<T> list)
		{
			var rnd = RandomProvider.Get();
			var max = list.Count();
			var i = rnd.Next(max);

			return list.ElementAt(i);
		}
	}
}
