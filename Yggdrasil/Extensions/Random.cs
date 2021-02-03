using System;

namespace Yggdrasil.Extensions
{
	/// <summary>
	/// Extensions to Random.
	/// </summary>
	public static class RandomExtensions
	{
		/// <summary>
		/// Returns a random number between min and max (incl).
		/// </summary>
		/// <param name="rnd"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public static float Between(this Random rnd, float min, float max)
		{
			return (float)(min + (rnd.NextDouble() * (max - min)));
		}

		/// <summary>
		/// Returns a random time span between the given ones based on
		/// their millisecond properties.
		/// </summary>
		/// <param name="rnd"></param>
		/// <param name="minTimeSpan"></param>
		/// <param name="maxTimeSpan"></param>
		/// <returns></returns>
		public static TimeSpan Between(this Random rnd, TimeSpan minTimeSpan, TimeSpan maxTimeSpan)
		{
			var min = (float)minTimeSpan.TotalMilliseconds;
			var max = (float)maxTimeSpan.TotalMilliseconds;

			return TimeSpan.FromMilliseconds(rnd.Between(min, max));
		}

		/// <summary>
		/// Returns a random value from the given ones.
		/// </summary>
		/// <param name="rnd"></param>
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
