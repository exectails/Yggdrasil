using System;
using System.Linq;
using Yggdrasil.Util;

namespace Yggdrasil.Geometry.Shapes
{
	/// <summary>
	/// A triangular base with a round top, akin to a flashlight's cone.
	/// </summary>
	public class Cone : IShape
	{
		private Vector2[] _edgePoints;
		private Outline[] _outlines;

		/// <summary>
		/// Returns the center of the cone.
		/// </summary>
		public Vector2 Center { get; }

		/// <summary>
		/// Returns the position of the pointed tip of the cone.
		/// </summary>
		public Vector2 Tip { get; }

		/// <summary>
		/// Returns the direction of the cone in degress, extending from
		/// the tip.
		/// </summary>
		public double Direction { get; }

		/// <summary>
		/// Returns the radius or "length" of the cone, from the tip to
		/// the rounded edge.
		/// </summary>
		public double Radius { get; }

		/// <summary>
		/// Returns the angle of the cone in degrees, from 0° (a line)
		/// to 180° (a half circle).
		/// </summary>
		public double Angle { get; }

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="tipPos">Position of the pointed tip of the cone.</param>
		/// <param name="direction">The direction of the cone in degrees, extending from the tip.</param>
		/// <param name="radius">The radius or "length" of the cone, from the tip to the rounded edge.</param>
		/// <param name="angle">The angle of the cone in degrees.</param>
		public Cone(Vector2 tipPos, double direction, double radius, float angle)
		{
			this.Tip = tipPos;
			this.Direction = direction;
			this.Radius = radius;
			this.Angle = Math2.Clamp(0, 180, angle);

			// Get center
			var radians = Math2.DegreeToRadian(direction);
			var centerDistance = radius / 2;

			var deltaX = Math.Cos(radians);
			var deltaY = Math.Sin(radians);
			var deltaXY = Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2));

			var centerX = tipPos.X + (centerDistance / deltaXY) * (deltaX);
			var centerY = tipPos.Y + (centerDistance / deltaXY) * (deltaY);

			this.Center = new Vector2((int)Math.Round(centerX), (int)Math.Round(centerY));
		}

		/// <summary>
		/// Returns true if the given position is within this cone.
		/// </summary>
		/// <param name="checkPos"></param>
		/// <returns></returns>
		public bool IsInside(Vector2 checkPos)
			=> IsInside(this.Tip, this.Radius, this.Direction, this.Angle, checkPos);

		/// <summary>
		/// Returns true if the given position is within this cone.
		/// </summary>
		/// <param name="tip">Position of the pointed tip of the cone.</param>
		/// <param name="direction">The direction of the cone in degrees, extending from the tip.</param>
		/// <param name="radius">The radius or "length" of the cone, from the tip to the rounded edge.</param>
		/// <param name="angle">The angle of the cone in degrees.</param>
		/// <param name="checkPos">The position to check whether it is inside the cone.</param>
		/// <returns></returns>
		public static bool IsInside(Vector2 tip, double radius, double direction, double angle, Vector2 checkPos)
		{
			if (angle <= 0)
				return false;

			direction = Math2.DegreeToRadian(direction);
			angle = Math2.DegreeToRadian(angle);

			var halfAngle = angle / 2;

			var tx1 = tip.X + (Math.Cos(-halfAngle + direction) * radius);
			var ty1 = tip.Y + (Math.Sin(-halfAngle + direction) * radius);
			var tx2 = tip.X + (Math.Cos(halfAngle + direction) * radius);
			var ty2 = tip.Y + (Math.Sin(halfAngle + direction) * radius);
			var tx3 = tip.X;
			var ty3 = tip.Y;
			var px = checkPos.X;
			var py = checkPos.Y;

			// Check if position is inside the triangle part of the cone.
			// http://stackoverflow.com/questions/2049582/how-to-determine-a-point-in-a-2d-triangle
			var A = 1.0 / 2.0 * (-ty2 * tx3 + ty1 * (-tx2 + tx3) + tx1 * (ty2 - ty3) + tx2 * ty3);
			var sign = (A < 0 ? -1 : 1);
			var s = (ty1 * tx3 - tx1 * ty3 + (ty3 - ty1) * px + (tx1 - tx3) * py) * sign;
			var t = (tx1 * ty2 - ty1 * tx2 + (ty1 - ty2) * px + (tx2 - tx1) * py) * sign;

			var isInTriangle = (s > 0 && t > 0 && (s + t) < 2 * A * sign);
			if (isInTriangle)
				return true;

			// Check if position is on the triangle part's side of the cone.
			// If it is, we can stop, since we already checked for the
			// triangle. The only way the position could now be in the cone
			// is if it was on the circle's side.
			// http://stackoverflow.com/a/3461533/1171898
			var isOnTriangleSide = (((tx2 - tx1) * (py - ty1) - (ty2 - ty1) * (px - tx1)) > 0);
			if (isOnTriangleSide)
				return false;

			// Check if position is inside the circle.
			// At one point we used a full circle at the top of the cone for
			// its rounded side, but now we're considering the cone to be
			// more of a slice of pizza.
			//var tx4 = (int)((tx1 + tx2) / 2);
			//var ty4 = (int)((ty1 + ty2) / 2);
			//var circleRadius = Math.Sqrt(Math.Pow(tx2 - tx1, 2) + Math.Pow(ty2 - ty1, 2)) / 2;
			//var targetDistance = Math.Sqrt(Math.Pow(tx4 - px, 2) + Math.Pow(ty4 - py, 2));
			var targetDistance = Math.Sqrt(Math.Pow(tip.X - checkPos.X, 2) + Math.Pow(tip.Y - checkPos.Y, 2));
			var isInCircle = (targetDistance < radius);

			return isInCircle;
		}

		/// <summary>
		/// Returns approximation of the edge points of the cone.
		/// </summary>
		/// <returns></returns>
		public Vector2[] GetEdgePoints()
		{
			if (_edgePoints == null)
			{
				var tip = this.Tip;
				var radius = this.Radius;
				var direction = Math2.DegreeToRadian(this.Direction);
				var angle = Math2.DegreeToRadian(this.Angle);

				var halfAngle = angle / 2;

				var tx1 = tip.X + (Math.Cos(-halfAngle + direction) * radius);
				var ty1 = tip.Y + (Math.Sin(-halfAngle + direction) * radius);
				var tx2 = tip.X + (Math.Cos(halfAngle + direction) * radius);
				var ty2 = tip.Y + (Math.Sin(halfAngle + direction) * radius);
				var tx5 = tip.X + (Math.Cos(direction) * radius);
				var ty5 = tip.Y + (Math.Sin(direction) * radius);

				_edgePoints = new Vector2[]
				{
					this.Tip,
					new Vector2((int)Math.Round(tx2), (int)Math.Round(ty2)),
					new Vector2((int)Math.Round(tx5), (int)Math.Round(ty5)),
					new Vector2((int)Math.Round(tx1), (int)Math.Round(ty1)),
				};
			}

			return _edgePoints;
		}

		/// <summary>
		/// Returns the cone's outline based on its edge points.
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
