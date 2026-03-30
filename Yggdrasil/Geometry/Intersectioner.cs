using System;
using Yggdrasil.Geometry.Shapes;

namespace Yggdrasil.Geometry
{
	/// <summary>
	/// A utility class for determining whether two shapes intersect.
	/// </summary>
	/// <remarks>
	/// Uses optimized methods for certain shape combinations where
	/// possible, such as circle/circle and circle/polygon, with a
	/// fallback to edge checks for other combinations.
	/// </remarks>
	public static class Intersectioner
	{
		/// <summary>
		/// Determines and returns whether the two shapes intersect.
		/// </summary>
		/// <param name="shape1"></param>
		/// <param name="shape2"></param>
		/// <returns></returns>
		public static bool Intersects(this IShapeF shape1, IShapeF shape2)
		{
			switch (shape1)
			{
				// Circle/Circle
				case CircleF circle1 when shape2 is CircleF circle2: return CircleIntersectsCircle(circle1, circle2);

				// Circle/Capsule
				case CircleF circle when shape2 is CapsuleF capsule: return CircleIntersectsCapsule(circle, capsule);
				case CapsuleF capsule when shape2 is CircleF circle: return CircleIntersectsCapsule(circle, capsule);

				// Circle/Cone
				case CircleF circle when shape2 is ConeF cone: return CircleIntersectsCone(circle, cone);
				case ConeF cone when shape2 is CircleF circle: return CircleIntersectsCone(circle, cone);

				// Circle/Polygon
				case CircleF circle when shape2 is PolygonF polygon: return CircleIntersectsPolygon(circle, polygon);
				case PolygonF polygon when shape2 is CircleF circle: return CircleIntersectsPolygon(circle, polygon);

				// Capsule/Polygon
				case CapsuleF capsule when shape2 is PolygonF polygon: return CapsuleIntersectsPolygon(capsule, polygon);
				case PolygonF polygon when shape2 is CapsuleF capsule: return CapsuleIntersectsPolygon(capsule, polygon);
			}

			// General purpose polygon fallback, using outlines
			return ShapesIntersect(shape1, shape2);
		}

		/// <summary>
		/// Determines and returns whether the shape intersects with the
		/// line.
		/// </summary>
		/// <param name="shape"></param>
		/// <param name="line"></param>
		/// <returns></returns>
		public static bool Intersects(this IShapeF shape, LineF line)
		{
			switch (shape)
			{
				case CircleF circle: return CircleIntersectsLine(circle, line);
				case CapsuleF capsule: return CapsuleIntersectsLine(capsule, line);
				case ConeF cone: return ConeIntersectsLine(cone, line);
				case PolygonF polygon: return PolygonIntersectsLine(polygon, line);
			}

			return ShapeIntersectsLine(shape, line);
		}

		/// <summary>
		/// Determines and returns whether the two shapes intersect.
		/// </summary>
		/// <param name="circle1"></param>
		/// <param name="circle2"></param>
		/// <returns></returns>
		public static bool Intersects(this CircleF circle1, CircleF circle2)
			=> CircleIntersectsCircle(circle1, circle2);

		/// <summary>
		/// Determines and returns whether the two shapes intersect.
		/// </summary>
		/// <param name="circle"></param>
		/// <param name="capsule"></param>
		/// <returns></returns>
		public static bool Intersects(this CircleF circle, CapsuleF capsule)
			=> CircleIntersectsCapsule(circle, capsule);

		/// <summary>
		/// Determines and returns whether the two shapes intersect.
		/// </summary>
		/// <param name="capsule"></param>
		/// <param name="circle"></param>
		/// <returns></returns>
		public static bool Intersects(this CapsuleF capsule, CircleF circle)
			=> CircleIntersectsCapsule(circle, capsule);

		/// <summary>
		/// Determines and returns whether the two shapes intersect.
		/// </summary>
		/// <param name="circle"></param>
		/// <param name="cone"></param>
		/// <returns></returns>
		public static bool Intersects(this CircleF circle, ConeF cone)
			=> CircleIntersectsCone(circle, cone);

		/// <summary>
		/// Determines and returns whether the two shapes intersect.
		/// </summary>
		/// <param name="cone"></param>
		/// <param name="circle"></param>
		/// <returns></returns>
		public static bool Intersects(this ConeF cone, CircleF circle)
			=> CircleIntersectsCone(circle, cone);

		/// <summary>
		/// Determines and returns whether the two shapes intersect.
		/// </summary>
		/// <param name="circle"></param>
		/// <param name="polygon"></param>
		/// <returns></returns>
		public static bool Intersects(this CircleF circle, PolygonF polygon)
			=> CircleIntersectsPolygon(circle, polygon);

		/// <summary>
		/// Determines and returns whether the two shapes intersect.
		/// </summary>
		/// <param name="polygon"></param>
		/// <param name="circle"></param>
		/// <returns></returns>
		public static bool Intersects(this PolygonF polygon, CircleF circle)
			=> CircleIntersectsPolygon(circle, polygon);

		/// <summary>
		/// Determines and returns whether the two shapes intersect.
		/// </summary>
		/// <param name="capsule"></param>
		/// <param name="polygon"></param>
		/// <returns></returns>
		public static bool Intersects(this CapsuleF capsule, PolygonF polygon)
			=> CapsuleIntersectsPolygon(capsule, polygon);

		/// <summary>
		/// Determines and returns whether the two shapes intersect.
		/// </summary>
		/// <param name="polygon"></param>
		/// <param name="capsule"></param>
		/// <returns></returns>
		public static bool Intersects(this PolygonF polygon, CapsuleF capsule)
			=> CapsuleIntersectsPolygon(capsule, polygon);

		/// <summary>
		/// Determines and returns whether the two circles intersect.
		/// </summary>
		/// <param name="circle1"></param>
		/// <param name="circle2"></param>
		/// <returns></returns>
		private static bool CircleIntersectsCircle(CircleF circle1, CircleF circle2)
		{
			var distance = circle1.Center.GetDistance(circle2.Center);
			var radiusSum = circle1.Radius + circle2.Radius;

			return distance <= radiusSum;
		}

		/// <summary>
		/// Determines and returns whether the circle and the capsule
		/// intersect.
		/// </summary>
		/// <param name="circle"></param>
		/// <param name="capsule"></param>
		/// <returns></returns>
		private static bool CircleIntersectsCapsule(CircleF circle, CapsuleF capsule)
		{
			var distSq = GetPointSegmentDistanceSq(circle.Center.X, circle.Center.Y, capsule.Point1.X, capsule.Point1.Y, capsule.Point2.X, capsule.Point2.Y);
			var radiusSum = circle.Radius + capsule.Radius;

			return distSq <= (radiusSum * radiusSum);
		}

		/// <summary>
		/// Determines and returns whether the circle intersects with the
		/// polygon in any way.
		/// </summary>
		/// <param name="circle"></param>
		/// <param name="polygon"></param>
		/// <returns></returns>
		private static bool CircleIntersectsPolygon(CircleF circle, PolygonF polygon)
		{
			if (polygon.IsInside(circle.Center))
				return true;

			var points = polygon.Points;
			var radiusSq = circle.Radius * circle.Radius;

			for (var i = 0; i < points.Length; ++i)
			{
				var p1 = points[i];
				var p2 = points[(i + 1) % points.Length];

				var distSq = GetPointSegmentDistanceSq(circle.Center.X, circle.Center.Y, p1.X, p1.Y, p2.X, p2.Y);
				if (distSq <= radiusSq)
					return true;
			}

			return false;
		}

		/// <summary>
		/// Determines and returns whether the capsule intersects with the
		/// polygon in any way.
		/// </summary>
		/// <param name="capsule"></param>
		/// <param name="polygon"></param>
		/// <returns></returns>
		private static bool CapsuleIntersectsPolygon(CapsuleF capsule, PolygonF polygon)
		{
			if (polygon.IsInside(capsule.Point1))
				return true;

			var points = polygon.Points;
			var radiusSq = capsule.Radius * capsule.Radius;

			var cx1 = capsule.Point1.X;
			var cy1 = capsule.Point1.Y;
			var cx2 = capsule.Point2.X;
			var cy2 = capsule.Point2.Y;

			for (var i = 0; i < points.Length; ++i)
			{
				var p1 = points[i];
				var p2 = points[(i + 1) % points.Length];

				// Check if the capsule's central line intersects with the
				// polygon edge
				if (SegmentsIntersect(cx1, cy1, cx2, cy2, p1.X, p1.Y, p2.X, p2.Y))
					return true;

				// Check distances from capsule endpoints to the polygon
				// edge
				if (GetPointSegmentDistanceSq(cx1, cy1, p1.X, p1.Y, p2.X, p2.Y) <= radiusSq)

					return true;
				if (GetPointSegmentDistanceSq(cx2, cy2, p1.X, p1.Y, p2.X, p2.Y) <= radiusSq)
					return true;

				// Check distance from the polygon edge endpoint to the
				// capsule segment
				if (GetPointSegmentDistanceSq(p1.X, p1.Y, cx1, cy1, cx2, cy2) <= radiusSq)
					return true;
			}

			return false;
		}

		/// <summary>
		/// Determines and returns whether the circle intersects with the
		/// cone in any way.
		/// </summary>
		/// <param name="circle"></param>
		/// <param name="cone"></param>
		/// <returns></returns>
		private static bool CircleIntersectsCone(CircleF circle, ConeF cone)
		{
			// Check whether the two circles intersect
			var tip = cone.Tip;
			var dx = circle.Center.X - tip.X;
			var dy = circle.Center.Y - tip.Y;
			var distanceSq = (dx * dx) + (dy * dy);
			var radiusSum = circle.Radius + (float)cone.Radius;

			if (distanceSq > radiusSum * radiusSum)
				return false;

			// Check if the circle's center is inside the cone
			if (cone.IsInside(circle.Center))
				return true;

			// Check intersection with the cone's straight edges
			var halfAngle = cone.Angle / 2.0;

			var leftAngle = (cone.Direction - halfAngle) * (Math.PI / 180.0);
			var leftX = tip.X + ((float)cone.Radius * (float)Math.Cos(leftAngle));
			var leftY = tip.Y + ((float)cone.Radius * (float)Math.Sin(leftAngle));

			var rightAngle = (cone.Direction + halfAngle) * (Math.PI / 180.0);
			var rightX = tip.X + ((float)cone.Radius * (float)Math.Cos(rightAngle));
			var rightY = tip.Y + ((float)cone.Radius * (float)Math.Sin(rightAngle));

			var circleRadiusSq = circle.Radius * circle.Radius;

			if (GetPointSegmentDistanceSq(circle.Center.X, circle.Center.Y, tip.X, tip.Y, leftX, leftY) <= circleRadiusSq)
				return true;

			if (GetPointSegmentDistanceSq(circle.Center.X, circle.Center.Y, tip.X, tip.Y, rightX, rightY) <= circleRadiusSq)
				return true;

			// Check intersection with the cone's round edge
			var distance = (float)Math.Sqrt(distanceSq);

			if (distance > circle.Radius && distance <= cone.Radius + circle.Radius)
			{
				var angleToCircleCenter = Math.Atan2(circle.Center.Y - tip.Y, circle.Center.X - tip.X) * (180.0 / Math.PI);

				var angleDiff = angleToCircleCenter - cone.Direction;
				while (angleDiff <= -180) angleDiff += 360;
				while (angleDiff > 180) angleDiff -= 360;

				var angularWidth = Math.Asin(circle.Radius / distance) * (180.0 / Math.PI);
				if (Math.Abs(angleDiff) <= halfAngle + angularWidth)
					return true;
			}

			return false;
		}

		/// <summary>
		/// Determines and returns whether the shapes intersect, based on
		/// a rough check of their outlines and edge points. Depending on
		/// the shapes, this may not be perfectly accurate.
		/// </summary>
		/// <param name="shape1"></param>
		/// <param name="shape2"></param>
		/// <returns></returns>
		public static bool ShapesIntersect(IShapeF shape1, IShapeF shape2)
		{
			var bounds1 = shape1.GetBounds();
			var bounds2 = shape2.GetBounds();

			// Can't intersect if their bounding boxes don't intersect.
			if (!bounds1.Intersects(bounds2))
				return false;

			var outlines1 = shape1.GetOutlines();
			var outlines2 = shape2.GetOutlines();

			// Check if any edges intersect
			foreach (var outline1 in outlines1)
			{
				foreach (var line1 in outline1.Lines)
				{
					foreach (var outline2 in outlines2)
					{
						foreach (var line2 in outline2.Lines)
						{
							if (line1.Intersects(line2, out _))
								return true;
						}
					}
				}
			}

			// Check if shape1 is inside shape2...
			foreach (var outline1 in outlines1)
			{
				if (outline1.Lines.Length > 0 && shape2.IsInside(outline1.Lines[0].Point1))
					return true;
			}

			// ... or vice versa
			foreach (var outline2 in outlines2)
			{
				if (outline2.Lines.Length > 0 && shape1.IsInside(outline2.Lines[0].Point1))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Determines and returns whether the circle intersects with the
		/// line in any way.
		/// </summary>
		/// <param name="circle"></param>
		/// <param name="line"></param>
		/// <returns></returns>
		private static bool CircleIntersectsLine(CircleF circle, LineF line)
		{
			return GetPointSegmentDistanceSq(circle.Center.X, circle.Center.Y, line.Point1.X, line.Point1.Y, line.Point2.X, line.Point2.Y) <= (circle.Radius * circle.Radius);
		}

		/// <summary>
		/// Determines and returns whether the capsule intersects with the
		/// line in any way.
		/// </summary>
		/// <param name="capsule"></param>
		/// <param name="line"></param>
		/// <returns></returns>
		private static bool CapsuleIntersectsLine(CapsuleF capsule, LineF line)
		{
			var cx1 = capsule.Point1.X;
			var cy1 = capsule.Point1.Y;
			var cx2 = capsule.Point2.X;
			var cy2 = capsule.Point2.Y;
			var lx1 = line.Point1.X;
			var ly1 = line.Point1.Y;
			var lx2 = line.Point2.X;
			var ly2 = line.Point2.Y;

			if (SegmentsIntersect(cx1, cy1, cx2, cy2, lx1, ly1, lx2, ly2))
				return true;

			var radiusSq = capsule.Radius * capsule.Radius;

			if (GetPointSegmentDistanceSq(cx1, cy1, lx1, ly1, lx2, ly2) <= radiusSq)
				return true;

			if (GetPointSegmentDistanceSq(cx2, cy2, lx1, ly1, lx2, ly2) <= radiusSq)
				return true;

			if (GetPointSegmentDistanceSq(lx1, ly1, cx1, cy1, cx2, cy2) <= radiusSq)
				return true;

			if (GetPointSegmentDistanceSq(lx2, ly2, cx1, cy1, cx2, cy2) <= radiusSq)
				return true;

			return false;
		}

		/// <summary>
		/// Determines and returns whether the cone intersects with the
		/// line in any way.
		/// </summary>
		/// <param name="cone"></param>
		/// <param name="line"></param>
		/// <returns></returns>
		private static bool ConeIntersectsLine(ConeF cone, LineF line)
		{
			if (!cone.GetBounds().Intersects(line.GetBoundingBox()))
				return false;

			if (cone.IsInside(line.Point1) || cone.IsInside(line.Point2))
				return true;

			var tip = cone.Tip;
			var halfAngle = cone.Angle / 2.0;

			var leftAngle = (cone.Direction - halfAngle) * (Math.PI / 180.0);
			var leftX = tip.X + ((float)cone.Radius * (float)Math.Cos(leftAngle));
			var leftY = tip.Y + ((float)cone.Radius * (float)Math.Sin(leftAngle));

			if (SegmentsIntersect(tip.X, tip.Y, leftX, leftY, line.Point1.X, line.Point1.Y, line.Point2.X, line.Point2.Y))
				return true;

			var rightAngle = (cone.Direction + halfAngle) * (Math.PI / 180.0);
			var rightX = tip.X + ((float)cone.Radius * (float)Math.Cos(rightAngle));
			var rightY = tip.Y + ((float)cone.Radius * (float)Math.Sin(rightAngle));

			if (SegmentsIntersect(tip.X, tip.Y, rightX, rightY, line.Point1.X, line.Point1.Y, line.Point2.X, line.Point2.Y))
				return true;

			var dx = line.Point2.X - line.Point1.X;
			var dy = line.Point2.Y - line.Point1.Y;
			var lengthSq = (dx * dx) + (dy * dy);

			if (lengthSq > 0)
			{
				var dot = (((tip.X - line.Point1.X) * dx) + ((tip.Y - line.Point1.Y) * dy)) / lengthSq;
				var t = Math.Max(0, Math.Min(1, dot));

				var closestX = line.Point1.X + (t * dx);
				var closestY = line.Point1.Y + (t * dy);

				var distSq = ((tip.X - closestX) * (tip.X - closestX)) + ((tip.Y - closestY) * (tip.Y - closestY));
				if (distSq <= cone.Radius * cone.Radius)
				{
					var angleToClosest = Math.Atan2(closestY - tip.Y, closestX - tip.X) * (180.0 / Math.PI);
					var angleDiff = angleToClosest - cone.Direction;
					while (angleDiff <= -180) angleDiff += 360;
					while (angleDiff > 180) angleDiff -= 360;

					if (Math.Abs(angleDiff) <= halfAngle)
						return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Determines and returns whether the polygon intersects with the
		/// line in any way.
		/// </summary>
		/// <param name="polygon"></param>
		/// <param name="line"></param>
		/// <returns></returns>
		private static bool PolygonIntersectsLine(PolygonF polygon, LineF line)
		{
			if (!polygon.GetBounds().Intersects(line.GetBoundingBox()))
				return false;

			if (polygon.IsInside(line.Point1) || polygon.IsInside(line.Point2))
				return true;

			var points = polygon.Points;
			for (var i = 0; i < points.Length; ++i)
			{
				var p1 = points[i];
				var p2 = points[(i + 1) % points.Length];

				if (SegmentsIntersect(p1.X, p1.Y, p2.X, p2.Y, line.Point1.X, line.Point1.Y, line.Point2.X, line.Point2.Y))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Determines and returns whether the shape intersects with the
		/// line in any way.
		/// </summary>
		/// <param name="shape"></param>
		/// <param name="line"></param>
		/// <returns></returns>
		private static bool ShapeIntersectsLine(IShapeF shape, LineF line)
		{
			if (!shape.GetBounds().Intersects(line.GetBoundingBox()))
				return false;

			if (shape.IsInside(line.Point1) || shape.IsInside(line.Point2))
				return true;

			foreach (var outline in shape.GetOutlines())
			{
				foreach (var outlineLine in outline.Lines)
				{
					if (line.Intersects(outlineLine, out _))
						return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Calculates the shortest squared distance between a point and a
		/// line segment.
		/// </summary>
		private static float GetPointSegmentDistanceSq(float px, float py, float sx1, float sy1, float sx2, float sy2)
		{
			var dx = sx2 - sx1;
			var dy = sy2 - sy1;
			var lengthSq = (dx * dx) + (dy * dy);

			var t = 0f;
			if (lengthSq > 0)
			{
				var dot = ((px - sx1) * dx) + ((py - sy1) * dy);
				t = Math.Max(0, Math.Min(1, dot / lengthSq));
			}

			var closestX = sx1 + (t * dx);
			var closestY = sy1 + (t * dy);

			return ((px - closestX) * (px - closestX)) + ((py - closestY) * (py - closestY));
		}

		/// <summary>
		/// Returns true if two line segments (a and b) intersect.
		/// </summary>
		private static bool SegmentsIntersect(float ax1, float ay1, float ax2, float ay2, float bx1, float by1, float bx2, float by2)
		{
			var denominator = ((ax1 - ax2) * (by1 - by2)) - ((ay1 - ay2) * (bx1 - bx2));
			if (denominator == 0)
				return false;

			var t = (((ax1 - bx1) * (by1 - by2)) - ((ay1 - by1) * (bx1 - bx2))) / denominator;
			var u = (((ax1 - bx1) * (ay1 - ay2)) - ((ay1 - by1) * (ax1 - ax2))) / denominator;

			return t >= 0 && t <= 1 && u >= 0 && u <= 1;
		}
	}
}
