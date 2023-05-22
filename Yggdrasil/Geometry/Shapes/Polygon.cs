using System;
using System.Collections.Generic;
using System.Linq;

namespace Yggdrasil.Geometry.Shapes
{
	/// <summary>
	/// A shape with an undefined number of edge points, but a minimum of 3,
	/// forming a triangle.
	/// </summary>
	public class Polygon : IShape
	{
		private Outline[] _outlines;

		/// <summary>
		/// Returns the polygon's position.
		/// </summary>
		public Vector2 Center { get; }

		/// <summary>
		/// Returns the polygon's points.
		/// </summary>
		public Vector2[] Points { get; }

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="points"></param>
		public Polygon(params Vector2[] points)
		{
			if (points.Count() < 3)
				throw new ArgumentException("A polygon needs at least 3 points.");

			this.Points = points;

			var count = points.Length;
			var xSum = points.Sum(a => a.X);
			var ySum = points.Sum(a => a.Y);
			var x = xSum / count;
			var y = ySum / count;

			this.Center = new Vector2(x, y);
		}

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="points"></param>
		public Polygon(IEnumerable<Vector2> points)
			: this(points.ToArray())
		{
		}

		/// <summary>
		/// Returns a polyon with 4 edge points, making up a rectangle
		/// with the given properties.
		/// </summary>
		/// <param name="x">Center position of the rectangle.</param>
		/// <param name="y">Center position of the rectangle.</param>
		/// <param name="width">Width of the rectangle.</param>
		/// <param name="height">Height of the rectangle.</param>
		/// <param name="rotation">Rotation of the rectangle in degrees.</param>
		/// <returns></returns>
		public static Polygon Rectangle(int x, int y, int width, int height, float rotation = 0)
		{
			return Rectangle(new Vector2(x, y), new Vector2(width, height), rotation);
		}

		/// <summary>
		/// Returns a polyon with 4 edge points, making up a rectangle
		/// with the given properties.
		/// </summary>
		/// <param name="center">Center position of the rectangle.</param>
		/// <param name="size">Size of the rectangle.</param>
		/// <param name="rotation">Rotation of the rectangle in degrees.</param>
		/// <returns></returns>
		public static Polygon Rectangle(Vector2 center, Vector2 size, float rotation)
		{
			var points = new Vector2[4];
			var width = size.X;
			var height = size.Y;

			points[0] = new Vector2(center.X - width / 2, center.Y + height / 2);
			points[1] = new Vector2(center.X + width / 2, center.Y + height / 2);
			points[2] = new Vector2(center.X + width / 2, center.Y - height / 2);
			points[3] = new Vector2(center.X - width / 2, center.Y - height / 2);

			PointUtil.Rotate(ref points, center, rotation);

			return new Polygon(points);
		}

		/// <summary>
		/// Returns a polygon with 4 edge points, making up a rectangle
		/// spanning from start to end.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="width"></param>
		/// <returns></returns>
		public static Polygon RectangleBetween(Vector2 start, Vector2 end, int width)
		{
			var points = new Vector2[4];

			var length = start.GetDistance(end);
			var halfWidth = width / 2;
			var dir = start.GetAngle(end);

			var x1 = start.X - halfWidth;
			var x2 = start.X + halfWidth;
			var y1 = start.Y;
			var y2 = (int)(start.Y + length);

			points[0] = new Vector2(x1, y1);
			points[1] = new Vector2(x2, y1);
			points[2] = new Vector2(x2, y2);
			points[3] = new Vector2(x1, y2);

			PointUtil.Rotate(ref points, start, dir);

			return new Polygon(points);
		}

		/// <summary>
		/// Returns true if the given point is within this polygon.
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public bool IsInside(Vector2 point)
		{
			if (this.Points.Length == 0)
				return false;

			var result = false;
			var points = this.Points;

			for (int i = 0, j = points.Length - 1; i < points.Length; j = i++)
			{
				if (((points[i].Y > point.Y) != (points[j].Y > point.Y)) && (point.X < (points[j].X - points[i].X) * (point.Y - points[i].Y) / (points[j].Y - points[i].Y) + points[i].X))
					result = !result;
			}

			return result;
		}

		/// <summary>
		/// Returns the polygon's edge points.
		/// </summary>
		/// <returns></returns>
		public Vector2[] GetEdgePoints()
		{
			return this.Points;
		}

		/// <summary>
		/// Returns the polygon's outline based on its edge points.
		/// </summary>
		/// <returns></returns>
		public Outline[] GetOutlines()
		{
			if (_outlines != null)
				return _outlines;

			var edgePoints = this.GetEdgePoints();
			var lines = new Line[edgePoints.Length];

			for (var i = 0; i < edgePoints.Length; ++i)
			{
				var point1 = edgePoints[i];
				var point2 = edgePoints[(i + 1) % edgePoints.Length];

				var line = new Line(point1, point2);
				lines[i] = line;
			}

			return _outlines = new Outline[] { new Outline(lines) };
		}

		/// <summary>
		/// Returns random point within the cone.
		/// </summary>
		/// <param name="rnd"></param>
		/// <returns></returns>
		public Vector2 GetRandomPoint(Random rnd)
		{
			var edgePoints = this.GetEdgePoints();
			var minX = edgePoints.Min(a => a.X);
			var maxX = edgePoints.Max(a => a.X);
			var minY = edgePoints.Min(a => a.Y);
			var maxY = edgePoints.Max(a => a.Y);

			for (var i = 0; i < 100; ++i)
			{
				var x = rnd.Next(minX, maxX + 1);
				var y = rnd.Next(minY, maxY + 1);
				var point = new Vector2(x, y);

				if (this.IsInside(point))
					return point;
			}

			return new Vector2(minX + (maxX - minX) / 2, minY + (maxY - minY) / 2);
		}
	}
}
