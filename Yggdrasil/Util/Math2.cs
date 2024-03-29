﻿using System;

namespace Yggdrasil.Util
{
	/// <summary>
	/// A few commonly used math-related functions.
	/// </summary>
	public static class Math2
	{
		/// <summary>
		/// Return the shorter of the two time spans.
		/// </summary>
		/// <param name="val1"></param>
		/// <param name="val2"></param>
		/// <returns></returns>
		public static TimeSpan Min(TimeSpan val1, TimeSpan val2)
		{
			return (val1 < val2 ? val1 : val2);
		}

		/// <summary>
		/// Return the longer of the two time spans.
		/// </summary>
		/// <param name="val1"></param>
		/// <param name="val2"></param>
		/// <returns></returns>
		public static TimeSpan Max(TimeSpan val1, TimeSpan val2)
		{
			return (val1 > val2 ? val1 : val2);
		}

		/// <summary>
		/// Return the earlier of the two date times.
		/// </summary>
		/// <param name="val1"></param>
		/// <param name="val2"></param>
		/// <returns></returns>
		public static DateTime Min(DateTime val1, DateTime val2)
		{
			return (val1 < val2 ? val1 : val2);
		}

		/// <summary>
		/// Return the later of the two date times.
		/// </summary>
		/// <param name="val1"></param>
		/// <param name="val2"></param>
		/// <returns></returns>
		public static DateTime Max(DateTime val1, DateTime val2)
		{
			return (val1 > val2 ? val1 : val2);
		}

		/// <summary>
		/// Returns min, if val is lower than min, max, if val is
		/// greater than max, or simply val.
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <param name="val"></param>
		/// <returns></returns>
		public static int Clamp(int min, int max, int val)
		{
			if (val < min)
				return min;
			if (val > max)
				return max;
			return val;
		}

		/// <summary>
		/// Returns min, if val is lower than min, max, if val is
		/// greater than max, or simply val.
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <param name="val"></param>
		/// <returns></returns>
		public static float Clamp(float min, float max, float val)
		{
			if (val < min)
				return min;
			if (val > max)
				return max;
			return val;
		}

		/// <summary>
		/// Returns min, if val is lower than min, max, if val is
		/// greater than max, or simply val.
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <param name="val"></param>
		/// <returns></returns>
		public static double Clamp(double min, double max, double val)
		{
			if (val < min)
				return min;
			if (val > max)
				return max;
			return val;
		}

		/// <summary>
		/// Returns min, if val is lower than min, max, if val is
		/// greater than max, or simply val.
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <param name="val"></param>
		/// <returns></returns>
		public static long Clamp(long min, long max, long val)
		{
			if (val < min)
				return min;
			if (val > max)
				return max;
			return val;
		}

		/// <summary>
		/// Returns min, if val is lower than min, max, if val is
		/// greater than max, or simply val.
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <param name="val"></param>
		/// <returns></returns>
		public static TimeSpan Clamp(TimeSpan min, TimeSpan max, TimeSpan val)
		{
			if (val < min)
				return min;
			if (val > max)
				return max;
			return val;
		}

		/// <summary>
		/// Returns true if val is between min and max (incl).
		/// </summary>
		/// <param name="val"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public static bool IsBetween(int val, int min, int max)
		{
			return (val >= min && val <= max);
		}

		/// <summary>
		/// Multiplies initial value with multiplier, returns either the
		/// result or Min/MaxValue if the multiplication caused an overflow.
		/// </summary>
		/// <param name="initialValue"></param>
		/// <param name="multiplier"></param>
		/// <returns></returns>
		public static short MultiplyChecked(short initialValue, double multiplier)
		{
			try
			{
				checked { return (short)(initialValue * multiplier); }
			}
			catch
			{
				if (initialValue >= 0)
					return short.MaxValue;
				else
					return short.MinValue;
			}
		}

		/// <summary>
		/// Multiplies initial value with multiplier, returns either the
		/// result or Min/MaxValue if the multiplication caused an overflow.
		/// </summary>
		/// <param name="initialValue"></param>
		/// <param name="multiplier"></param>
		/// <returns></returns>
		public static int MultiplyChecked(int initialValue, double multiplier)
		{
			try
			{
				checked { return (int)(initialValue * multiplier); }
			}
			catch
			{
				if (initialValue >= 0)
					return int.MaxValue;
				else
					return int.MinValue;
			}
		}

		/// <summary>
		/// Multiplies initial value with multiplier, returns either the
		/// result or Min/MaxValue if the multiplication caused an overflow.
		/// </summary>
		/// <param name="initialValue"></param>
		/// <param name="multiplier"></param>
		/// <returns></returns>
		public static long MultiplyChecked(long initialValue, double multiplier)
		{
			try
			{
				checked { return (long)(initialValue * multiplier); }
			}
			catch
			{
				if (initialValue >= 0)
					return long.MaxValue;
				else
					return long.MinValue;
			}
		}

		/// <summary>
		/// Adds addend to value, returns either the result or Min/MaxValue
		/// if the addition caused an overflow.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="addend"></param>
		/// <returns></returns>
		public static short AddChecked(short value, short addend)
		{
			try
			{
				checked { return (short)(value + addend); }
			}
			catch
			{
				if (addend >= 0)
					return short.MaxValue;
				else
					return short.MinValue;
			}
		}

		/// <summary>
		/// Adds addend to value, returns either the result or Min/MaxValue
		/// if the addition caused an overflow.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="addend"></param>
		/// <returns></returns>
		public static int AddChecked(int value, int addend)
		{
			try
			{
				checked { return (value + addend); }
			}
			catch
			{
				if (addend >= 0)
					return int.MaxValue;
				else
					return int.MinValue;
			}
		}

		/// <summary>
		/// Adds addend to value, returns either the result or Min/MaxValue
		/// if the addition caused an overflow.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="addend"></param>
		/// <returns></returns>
		public static long AddChecked(long value, long addend)
		{
			try
			{
				checked { return (value + addend); }
			}
			catch
			{
				if (addend >= 0)
					return long.MaxValue;
				else
					return long.MinValue;
			}
		}

		/// <summary>
		/// Returns the given degree in radians.
		/// </summary>
		/// <param name="degree"></param>
		/// <returns></returns>
		public static float DegreeToRadian(double degree)
		{
			return (float)(Math.PI / 180f * degree);
		}

		/// <summary>
		/// Returns the given radian in degree.
		/// </summary>
		/// <param name="radian"></param>
		/// <returns></returns>
		public static float RadianToDegree(double radian)
		{
			return (float)(radian * (180f / Math.PI));
		}
	}
}
