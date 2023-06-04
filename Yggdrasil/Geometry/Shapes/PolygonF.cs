using System;
using System.Collections.Generic;
using System.Linq;
using Yggdrasil.Extensions;

namespace Yggdrasil.Geometry.Shapes
{
	/// <summary>
	/// A shape with an undefined number of edge points, but a minimum of 3,
	/// forming a triangle.
	/// </summary>
	public class PolygonF : IShapeF
	{
		private OutlineF[] _outlines;

		/// <summary>
		/// Returns the polygon's position.
		/// </summary>
		public Vector2F Center { get; }

		/// <summary>
		/// Returns the polygon's points.
		/// </summary>
		public Vector2F[] Points { get; }

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="points"></param>
		public PolygonF(params Vector2F[] points)
		{
			if (points.Count() < 3)
				throw new ArgumentException("A polygon needs at least 3 points.");

			this.Points = points;

			var count = points.Length;
			var xSum = points.Sum(a => a.X);
			var ySum = points.Sum(a => a.Y);
			var x = xSum / count;
			var y = ySum / count;

			this.Center = new Vector2F(x, y);
		}

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="points"></param>
		public PolygonF(IEnumerable<Vector2F> points)
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
		public static PolygonF Rectangle(int x, int y, int width, int height, float rotation = 0)
		{
			return Rectangle(new Vector2F(x, y), new Vector2F(width, height), rotation);
		}

		/// <summary>
		/// Returns a polyon with 4 edge points, making up a rectangle
		/// with the given properties.
		/// </summary>
		/// <param name="center">Center position of the rectangle.</param>
		/// <param name="size">Size of the rectangle.</param>
		/// <param name="rotation">Rotation of the rectangle in degrees.</param>
		/// <returns></returns>
		public static PolygonF Rectangle(Vector2F center, Vector2F size, float rotation)
		{
			var points = new Vector2F[4];
			var width = size.X;
			var height = size.Y;

			points[0] = new Vector2F(center.X - width / 2, center.Y + height / 2);
			points[1] = new Vector2F(center.X + width / 2, center.Y + height / 2);
			points[2] = new Vector2F(center.X + width / 2, center.Y - height / 2);
			points[3] = new Vector2F(center.X - width / 2, center.Y - height / 2);

			PointUtil.Rotate(ref points, center, rotation);

			return new PolygonF(points);
		}

		/// <summary>
		/// Returns a polygon with 4 edge points, making up a rectangle
		/// spanning from start to end.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="width"></param>
		/// <returns></returns>
		public static PolygonF RectangleBetween(Vector2F start, Vector2F end, float width)
		{
			var points = new Vector2F[4];

			var length = start.GetDistance(end);
			var halfWidth = width / 2;
			var dir = start.GetAngle(end);

			var x1 = start.X - halfWidth;
			var x2 = start.X + halfWidth;
			var y1 = start.Y;
			var y2 = (int)(start.Y + length);

			points[0] = new Vector2F(x1, y1);
			points[1] = new Vector2F(x2, y1);
			points[2] = new Vector2F(x2, y2);
			points[3] = new Vector2F(x1, y2);

			PointUtil.Rotate(ref points, start, dir);

			return new PolygonF(points);
		}

		/// <summary>
		/// Returns true if the given point is within this polygon.
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public bool IsInside(Vector2F point)
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
		public Vector2F[] GetEdgePoints()
		{
			return this.Points;
		}

		/// <summary>
		/// Returns the polygon's outline based on its edge points.
		/// </summary>
		/// <returns></returns>
		public OutlineF[] GetOutlines()
		{
			if (_outlines != null)
				return _outlines;

			var edgePoints = this.GetEdgePoints();
			var lines = new LineF[edgePoints.Length];

			for (var i = 0; i < edgePoints.Length; ++i)
			{
				var point1 = edgePoints[i];
				var point2 = edgePoints[(i + 1) % edgePoints.Length];

				var line = new LineF(point1, point2);
				lines[i] = line;
			}

			return _outlines = new OutlineF[] { new OutlineF(lines) };
		}

		/// <summary>
		/// Returns random point within the cone.
		/// </summary>
		/// <param name="rnd"></param>
		/// <returns></returns>
		public Vector2F GetRandomPoint(Random rnd)
		{
			var edgePoints = this.GetEdgePoints();
			var minX = edgePoints.Min(a => a.X);
			var maxX = edgePoints.Max(a => a.X);
			var minY = edgePoints.Min(a => a.Y);
			var maxY = edgePoints.Max(a => a.Y);

			for (var i = 0; i < 100; ++i)
			{
				var x = rnd.Between(minX, maxX);
				var y = rnd.Between(minY, maxY);
				var point = new Vector2F(x, y);

				if (this.IsInside(point))
					return point;
			}

			return new Vector2F(minX + (maxX - minX) / 2, minY + (maxY - minY) / 2);
		}
	}
}
