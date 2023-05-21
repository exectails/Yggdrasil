using System;
using Yggdrasil.Util;

namespace Yggdrasil.Geometry
{
	/// <summary>
	/// Provides utility functions to modify points.
	/// </summary>
	public static class PointUtil
	{
		/// <summary>
		/// Returns new point that was rotated by the given amount
		/// around pivot.
		/// </summary>
		/// <param name="point"></param>
		/// <param name="pivot"></param>
		/// <param name="degrees"></param>
		/// <returns></returns>
		public static Vector2 Rotate(Vector2 point, Vector2 pivot, double degrees)
		{
			var radians = Math2.DegreeToRadian(degrees);
			var cosTheta = Math.Cos(radians);
			var sinTheta = Math.Sin(radians);

			var x = (int)(cosTheta * (point.X - pivot.X) - sinTheta * (point.Y - pivot.Y) + pivot.X);
			var y = (int)(sinTheta * (point.X - pivot.X) + cosTheta * (point.Y - pivot.Y) + pivot.Y);

			return new Vector2(x, y);
		}

		/// <summary>
		/// Returns new point that was rotated by the given amount
		/// around pivot.
		/// </summary>
		/// <param name="point"></param>
		/// <param name="pivot"></param>
		/// <param name="degrees"></param>
		/// <returns></returns>
		public static Vector2F Rotate(Vector2F point, Vector2F pivot, double degrees)
		{
			var radians = Math2.DegreeToRadian(degrees);
			var cosTheta = Math.Cos(radians);
			var sinTheta = Math.Sin(radians);

			var x = (float)(cosTheta * (point.X - pivot.X) - sinTheta * (point.Y - pivot.Y) + pivot.X);
			var y = (float)(sinTheta * (point.X - pivot.X) + cosTheta * (point.Y - pivot.Y) + pivot.Y);

			return new Vector2F(x, y);
		}

		/// <summary>
		/// Rotates points by the given amount around pivot.
		/// </summary>
		/// <remarks>
		/// The rotation is continually, not fixed, meaning a degree of
		/// 90 doesn't rotate the points "to" 90 degree, but that the 90
		/// degrees are "added" to the current rotation of the points.
		/// There is no base rotation.
		/// </remarks>
		/// <param name="points"></param>
		/// <param name="pivot"></param>
		/// <param name="degrees"></param>
		public static void Rotate(ref Vector2[] points, Vector2 pivot, double degrees)
		{
			for (var i = 0; i < points.Length; ++i)
				points[i] = Rotate(points[i], pivot, degrees);
		}

		/// <summary>
		/// Rotates points by the given amount around pivot.
		/// </summary>
		/// <remarks>
		/// The rotation is continually, not fixed, meaning a degree of
		/// 90 doesn't rotate the points "to" 90 degree, but that the 90
		/// degrees are "added" to the current rotation of the points.
		/// There is no base rotation.
		/// </remarks>
		/// <param name="points"></param>
		/// <param name="pivot"></param>
		/// <param name="degrees"></param>
		public static void Rotate(ref Vector2F[] points, Vector2F pivot, double degrees)
		{
			for (var i = 0; i < points.Length; ++i)
				points[i] = Rotate(points[i], pivot, degrees);
		}
	}
}
