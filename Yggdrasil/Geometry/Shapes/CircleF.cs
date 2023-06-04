using System;
using Yggdrasil.Extensions;

namespace Yggdrasil.Geometry.Shapes
{
	/// <summary>
	/// A round shape.
	/// </summary>
	public class CircleF : IShapeF
	{
		private Vector2F[] _edgePoints;
		private OutlineF[] _outlines;

		private const float PerUnitEdge = 200.0f;
		private const int MinEdges = 7;
		private const int MaxEdges = 16;

		/// <summary>
		/// Returns the circle's center position.
		/// </summary>
		public Vector2F Center { get; }

		/// <summary>
		/// Returns the circle's radius.
		/// </summary>
		public float Radius { get; }

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="center"></param>
		/// <param name="radius"></param>
		public CircleF(Vector2F center, float radius)
		{
			this.Center = center;
			this.Radius = radius;
		}

		/// <summary>
		/// Returns true if the given position is within this circle.
		/// </summary>
		/// <param name="pos"></param>
		/// <returns></returns>
		public bool IsInside(Vector2F pos)
		{
			if (this.Center == pos)
				return true;

			var distance = Math.Sqrt(Math.Pow(this.Center.X - pos.X, 2) + Math.Pow(this.Center.Y - pos.Y, 2));
			return distance < this.Radius;
		}

		/// <summary>
		/// Returns the number of edges to use by default, based on the size
		/// of circle.
		/// </summary>
		/// <returns></returns>
		private int GetDefaultEdges()
		{
			// Use X edges per Y units by default, with a minimum of X.
			// For example, with a radius of 300, unit edge 200, and min
			// edge 7, the number of edges would be 10.

			var edges = (int)Math.Min(MaxEdges, (Math.Max(1, this.Radius / PerUnitEdge) * MinEdges));
			return edges;
		}

		/// <summary>
		/// Returns an aproximation for the circle's edge points.
		/// </summary>
		/// <returns></returns>
		public Vector2F[] GetEdgePoints()
		{
			if (_edgePoints == null)
				_edgePoints = this.GetEdgePoints(this.GetDefaultEdges());

			return _edgePoints;
		}

		/// <summary>
		/// Returns the circle's outline based on its edge points.
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
		/// Returns the given amount of edge points for this circle.
		/// </summary>
		/// <param name="edges"></param>
		/// <returns></returns>
		public Vector2F[] GetEdgePoints(int edges)
		{
			var center = new Vector2F(this.Center.X, this.Center.Y);
			var radius = this.Radius;
			var result = new Vector2F[edges];
			var step = 360.0 / edges;

			var point = new Vector2F(center.X, center.Y + radius);
			for (var i = 0; i < edges; ++i)
			{
				point = PointUtil.Rotate(point, center, step);
				result[i] = new Vector2F((float)point.X, (float)point.Y);
			}

			return result;
		}

		/// <summary>
		/// Returns a random point within the circle.
		/// </summary>
		/// <param name="rnd"></param>
		/// <returns></returns>
		public Vector2F GetRandomPoint(Random rnd)
		{
			var distance = rnd.Between(0, this.Radius);
			var angle = rnd.NextDouble() * Math.PI * 2;
			var x = this.Center.X + distance * Math.Cos(angle);
			var y = this.Center.Y + distance * Math.Sin(angle);

			return new Vector2F((float)x, (float)y);
		}
	}
}
