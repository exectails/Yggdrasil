using System;

namespace Yggdrasil.Geometry.Shapes
{
	/// <summary>
	/// A capsule shape, aka a rectangle with semicircular ends, defined
	/// by two points and a radius.
	/// </summary>
	public class CapsuleF : IShapeF
	{
		private Vector2F[] _edgePoints;
		private OutlineF[] _outlines;

		/// <summary>
		/// Returns the capsule's center position.
		/// </summary>
		public Vector2F Center { get; private set; }

		/// <summary>
		/// Returns the capsule's first point, which is the center of one
		/// of the circular ends.
		/// </summary>
		public Vector2F Point1 { get; private set; }

		/// <summary>
		/// Returns the capsule's second point, which is the center of one
		/// of the circular ends.
		/// </summary>
		public Vector2F Point2 { get; private set; }

		/// <summary>
		/// Returns the capsule's radius, which is the distance from the
		/// center to the edge, and the radius of the circular ends.
		/// </summary>
		public float Radius { get; }

		/// <summary>
		/// Returns the capsule's length, which is the distance between
		/// the centers of the circular ends, and the length of the
		/// rectangular middle section.
		/// </summary>
		public float Length { get; }

		/// <summary>
		/// Creates new capsule.
		/// </summary>
		/// <param name="point1"></param>
		/// <param name="point2"></param>
		/// <param name="radius"></param>
		/// <param name="length"></param>
		public CapsuleF(Vector2F point1, Vector2F point2, float radius, float length)
		{
			this.Point1 = point1;
			this.Point2 = point2;
			this.Radius = radius;
			this.Length = length;

			this.Center = new Vector2F((point1.X + point2.X) / 2, (point1.Y + point2.Y) / 2);
		}

		/// <summary>
		/// Returns a bounding box that fully contains the capsule.
		/// </summary>
		/// <returns></returns>
		public BoundingBoxF GetBounds()
		{
			var minX = Math.Min(this.Point1.X, this.Point2.X) - this.Radius;
			var minY = Math.Min(this.Point1.Y, this.Point2.Y) - this.Radius;
			var maxX = Math.Max(this.Point1.X, this.Point2.X) + this.Radius;
			var maxY = Math.Max(this.Point1.Y, this.Point2.Y) + this.Radius;

			return BoundingBoxF.FromLTRB(minX, minY, maxX, maxY);
		}

		/// <summary>
		/// Returns an approximation of the capsule's edge points.
		/// </summary>
		/// <returns></returns>
		public Vector2F[] GetEdgePoints()
		{
			if (_edgePoints != null)
				return _edgePoints;

			var dirX = this.Point2.X - this.Point1.X;
			var dirY = this.Point2.Y - this.Point1.Y;
			var length = (float)Math.Sqrt(dirX * dirX + dirY * dirY);

			var dirNX = length > 0 ? dirX / length : 1;
			var dirNY = length > 0 ? dirY / length : 0;

			var perpX = -dirNY;
			var perpY = dirNX;

			return _edgePoints = new Vector2F[]
			{
				new Vector2F(this.Point1.X - dirNX * this.Radius, this.Point1.Y - dirNY * this.Radius),
				new Vector2F(this.Point1.X + perpX * this.Radius, this.Point1.Y + perpY * this.Radius),
				new Vector2F(this.Point2.X + perpX * this.Radius, this.Point2.Y + perpY * this.Radius),
				new Vector2F(this.Point2.X + dirNX * this.Radius, this.Point2.Y + dirNY * this.Radius),
				new Vector2F(this.Point2.X - perpX * this.Radius, this.Point2.Y - perpY * this.Radius),
				new Vector2F(this.Point1.X - perpX * this.Radius, this.Point1.Y - perpY * this.Radius),
			};
		}

		/// <summary>
		/// Returns the outline of the capsule based on its rough edge
		/// points.
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
		/// Returns a random point inside the capsule.
		/// </summary>
		/// <param name="rnd"></param>
		/// <returns></returns>
		public Vector2F GetRandomPoint(Random rnd)
		{
			var bounds = this.GetBounds();

			for (var i = 0; i < 10; ++i)
			{
				var x = (float)(bounds.Left + rnd.NextDouble() * bounds.Width);
				var y = (float)(bounds.Top + rnd.NextDouble() * bounds.Height);

				var point = new Vector2F(x, y);

				if (this.IsInside(point))
					return point;
			}

			return this.Center;
		}

		/// <summary>
		/// Returns true if the given point is inside the capsule.
		/// </summary>
		/// <param name="pos"></param>
		/// <returns></returns>
		public bool IsInside(Vector2F pos)
		{
			if (pos.InRange(this.Point1, this.Radius))
				return true;

			if (pos.InRange(this.Point2, this.Radius))
				return true;

			var dirX = this.Point2.X - this.Point1.X;
			var dirY = this.Point2.Y - this.Point1.Y;

			var length = (float)Math.Sqrt(dirX * dirX + dirY * dirY);
			if (length == 0)
				return false;

			var dirNX = dirX / length;
			var dirNY = dirY / length;

			var relX = pos.X - this.Point1.X;
			var relY = pos.Y - this.Point1.Y;

			var projLength = relX * dirNX + relY * dirNY;
			if (projLength < 0 || projLength > length)
				return false;

			// Calculate the closest point on the line segment
			var projX = this.Point1.X + dirNX * projLength;
			var projY = this.Point1.Y + dirNY * projLength;

			// Check if the distance to the line segment is within the radius
			var distToLineSq = (pos.X - projX) * (pos.X - projX) + (pos.Y - projY) * (pos.Y - projY);

			return distToLineSq <= this.Radius * this.Radius;
		}

		/// <summary>
		/// Updates the capsule's position by moving its center position.
		/// </summary>
		/// <param name="position"></param>
		public void UpdatePosition(Vector2F position)
		{
			var deltaX = position.X - this.Center.X;
			var deltaY = position.Y - this.Center.Y;

			this.Point1 = new Vector2F(this.Point1.X + deltaX, this.Point1.Y + deltaY);
			this.Point2 = new Vector2F(this.Point2.X + deltaX, this.Point2.Y + deltaY);
			this.Center = position;

			_edgePoints = null;
			_outlines = null;
		}
	}
}
