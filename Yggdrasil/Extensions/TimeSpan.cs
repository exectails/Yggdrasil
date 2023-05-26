using System;

namespace Yggdrasil.Extensions
{
	/// <summary>
	/// Extensions for TimeSpan.
	/// </summary>
	public static class TimeSpanExtensions
	{
		/// <summary>
		/// Divides the timespan by the divisor and returns the result.
		/// </summary>
		/// <param name="timespan"></param>
		/// <param name="divisor"></param>
		/// <returns></returns>
		public static TimeSpan Divide(this TimeSpan timespan, int divisor)
			=> TimeSpan.FromTicks(timespan.Ticks / divisor);

		/// <summary>
		/// Multiplies the timespan by the multiplier and returns the result.
		/// </summary>
		/// <param name="timespan"></param>
		/// <param name="multiplier"></param>
		/// <returns></returns>
		public static TimeSpan Multiply(this TimeSpan timespan, int multiplier)
			=> TimeSpan.FromTicks(timespan.Ticks * multiplier);
	}
}
