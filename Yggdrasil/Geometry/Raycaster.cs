using System;
using System.Collections.Generic;
using Yggdrasil.Geometry.Shapes;

namespace Yggdrasil.Geometry
{
	/// <summary>
	/// A utility class for performing raycasting operations against
	/// shapes.
	/// </summary>
	/// <remarks>
	/// Currently only supports circles and polygons/rectangles.
	/// </remarks>
	public static class Raycaster
	{
		/// <summary>
		/// Casts a ray against a collection of shapes and returns the
		/// closest hit.
		/// </summary>
		public static RaycastResult Raycast(Vector2F origin, Vector2F direction, IReadOnlyList<IShapeF> shapes)
		{
			direction = Vector2F.Normalize(direction);

			var closestHit = new RaycastResult { Hit = false, Distance = float.MaxValue };

			foreach (var shape in shapes)
			{
				var hit = Raycast(origin, direction, shape);
				if (hit.Hit && hit.Distance < closestHit.Distance)
					closestHit = hit;
			}

			if (!closestHit.Hit)
				closestHit.Distance = 0;

			return closestHit;
		}

		/// <summary>
		/// Casts a ray against a single shape.
		/// </summary>
		public static RaycastResult Raycast(Vector2F origin, Vector2F direction, IShapeF shape)
		{
			direction = Vector2F.Normalize(direction);

			switch (shape)
			{
				case CircleF circle: return RaycastCircle(origin, direction, circle);
				case PolygonF polygon: return RaycastPolygon(origin, direction, polygon);

				default: return RaycastResult.NoHit;
			}
		}

		/// <summary>
		/// Casts a ray against a circle and returns the hit result.
		/// </summary>
		/// <param name="origin"></param>
		/// <param name="direction"></param>
		/// <param name="circle"></param>
		/// <returns></returns>
		private static RaycastResult RaycastCircle(Vector2F origin, Vector2F direction, CircleF circle)
		{
			var toCircle = circle.Center - origin;
			var projectionLength = Vector2F.Dot(toCircle, direction);
			var closestPoint = origin + direction * projectionLength;

			var distanceToCircle = Vector2F.Distance(closestPoint, circle.Center);
			if (distanceToCircle > circle.Radius)
				return RaycastResult.NoHit;

			var offset = (float)Math.Sqrt(circle.Radius * circle.Radius - distanceToCircle * distanceToCircle);
			var hitDistance = projectionLength - offset;
			if (hitDistance < 0)
			{
				hitDistance = projectionLength + offset;
				if (hitDistance < 0)
					return RaycastResult.NoHit;
			}

			var hitPoint = origin + direction * hitDistance;
			var normal = Vector2F.Normalize(hitPoint - circle.Center);

			return new RaycastResult
			{
				Hit = true,
				Distance = hitDistance,
				Point = hitPoint,
				Normal = normal,
				Shape = circle,
			};
		}

		/// <summary>
		/// Returns the closest hit of a ray against a polygon by checking
		/// each edge for intersection.
		/// </summary>
		/// <param name="origin"></param>
		/// <param name="direction"></param>
		/// <param name="polygon"></param>
		/// <returns></returns>
		private static RaycastResult RaycastPolygon(Vector2F origin, Vector2F direction, PolygonF polygon)
		{
			var closestHit = new RaycastResult { Hit = false, Distance = float.MaxValue };

			var points = polygon.Points;
			for (var i = 0; i < points.Length; i++)
			{
				var start = points[i];
				var end = points[(i + 1) % points.Length];

				var edgeResult = RaycastEdge(origin, direction, start, end);
				if (edgeResult.Hit && edgeResult.Distance < closestHit.Distance)
				{
					closestHit = edgeResult;
					closestHit.Shape = polygon;
				}
			}

			if (!closestHit.Hit)
				closestHit.Distance = 0;

			return closestHit;
		}

		/// <summary>
		/// Casts a ray against a single edge defined by two points and
		/// returns the hit result.
		/// </summary>
		/// <param name="origin"></param>
		/// <param name="direction"></param>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <returns></returns>
		private static RaycastResult RaycastEdge(Vector2F origin, Vector2F direction, Vector2F start, Vector2F end)
		{
			var edge = end - start;
			var edgeNormal = new Vector2F(-edge.Y, edge.X);
			var denominator = Vector2F.Dot(edgeNormal, direction);

			if (denominator > 0)
			{
				edgeNormal = new Vector2F(edge.Y, -edge.X);
				denominator = -denominator;
			}

			if (Math.Abs(denominator) < 1e-6)
				return RaycastResult.NoHit;

			var t = Vector2F.Dot(edgeNormal, start - origin) / denominator;
			if (t < 0)
				return RaycastResult.NoHit;

			var hitPoint = origin + direction * t;
			var edgeLengthSquared = Vector2F.Dot(edge, edge);

			var projection = Vector2F.Dot(hitPoint - start, edge);
			if (projection < 0 || projection > edgeLengthSquared)
				return RaycastResult.NoHit;

			var normal = Vector2F.Normalize(edgeNormal);

			return new RaycastResult
			{
				Hit = true,
				Distance = t,
				Point = hitPoint,
				Normal = normal,
				Shape = null,
			};
		}
	}

	/// <summary>
	/// The result of a raycast operation, containing information about a
	/// potential hit.
	/// </summary>
	public struct RaycastResult
	{
		/// <summary>
		/// Indicates whether the raycast hit a shape.
		/// </summary>
		public bool Hit;

		/// <summary>
		/// The distance from the ray's origin to the hit point, if a hit
		/// occurred.
		/// </summary>
		public float Distance;

		/// <summary>
		/// The point of intersection where the ray hit the shape, if a hit
		/// occurred.
		/// </summary>
		public Vector2F Point;

		/// <summary>
		/// The normal vector at the point of intersection, pointing away
		/// from the shape's surface, if a hit occurred.
		/// </summary>
		public Vector2F Normal;

		/// <summary>
		/// The shape that was hit by the ray, if a hit occurred.
		/// </summary>
		public IShapeF Shape;

		/// <summary>
		/// A raycast result representing a miss.
		/// </summary>
		public static readonly RaycastResult NoHit = new RaycastResult { Hit = false, Distance = 0 };
	}
}
