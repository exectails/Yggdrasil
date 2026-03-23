using System;

namespace Yggdrasil.Geometry
{
	/// <summary>
	/// An axis-aligned, floating-point bounding box, representing a
	/// rectangular area in 2D space.
	/// </summary>
	public readonly struct BoundingBoxF : IEquatable<BoundingBoxF>
	{
		/// <summary>
		/// The X coordinate, representing the left edge of the bounding
		/// box.
		/// </summary>
		public readonly float X;

		/// <summary>
		/// The Y coordinate, representing the top edge of the bounding
		/// box.
		/// </summary>
		public readonly float Y;

		/// <summary>
		/// The width of the bounding box, representing the distance from
		/// the left edge to the right edge.
		/// </summary>
		public readonly float Width;

		/// <summary>
		/// The height of the bounding box, representing the distance from
		/// the top edge to the bottom edge.
		/// </summary>
		public readonly float Height;

		/// <summary>
		/// Returns the left edge of the bounding box, which is the same
		/// as the X coordinate.
		/// </summary>
		public float Left => X;

		/// <summary>
		/// Returns the top edge of the bounding box, which is the same as
		/// the Y coordinate.
		/// </summary>
		public float Top => Y;

		/// <summary>
		/// Returns the right edge of the bounding box, calculated from X
		/// and Width.
		/// </summary>
		public float Right => X + Width;

		/// <summary>
		/// Returns the bottom edge of the bounding box, calculated from Y
		/// and Height.
		/// </summary>
		public float Bottom => Y + Height;

		/// <summary>
		/// Creates and returns a new bounding box with all properties set
		/// to zero.
		/// </summary>
		public static BoundingBoxF Empty { get; } = default;

		/// <summary>
		/// Returns true if the bounding box has zero width and height.
		/// </summary>
		public bool IsEmpty => this.Width == 0 && this.Height == 0;

		/// <summary>
		/// Creates new bounding box.
		/// </summary>
		/// <param name="pos"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		public BoundingBoxF(Vector2F pos, float width, float height)
			: this(pos.X, pos.Y, width, height)
		{
		}

		/// <summary>
		/// Creates new bounding box.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		public BoundingBoxF(float x, float y, float width, float height)
		{
			this.X = x;
			this.Y = y;
			this.Width = width;
			this.Height = height;
		}

		/// <summary>
		/// Returns a bounding box that contains all of the given points.
		/// </summary>
		/// <param name="points"></param>
		/// <returns></returns>
		public static BoundingBoxF FromPoints(ReadOnlySpan<Vector2F> points)
		{
			if (points.Length == 0)
				return Empty;

			var left = points[0].X;
			var top = points[0].Y;
			var right = points[0].X;
			var bottom = points[0].Y;

			for (var i = 1; i < points.Length; ++i)
			{
				var point = points[i];

				if (point.X < left) left = point.X;
				else if (point.X > right) right = point.X;

				if (point.Y < top) top = point.Y;
				else if (point.Y > bottom) bottom = point.Y;
			}

			return new BoundingBoxF(left, top, right - left, bottom - top);
		}

		/// <summary>
		/// Returns true if the given bounding box intersects with this
		/// one, meaning that parts of them overlap or touch.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Intersects(BoundingBoxF other)
		{
			return this.Left <= other.Right && this.Right >= other.Left && this.Top <= other.Bottom && this.Bottom >= other.Top;
		}

		/// <summary>
		/// Returns true if the given point is contained within this
		/// bounding box.
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public bool Contains(Vector2F point)
			=> this.Contains(point.X, point.Y);

		/// <summary>
		/// Returns true if the given point is contained within this
		/// bounding box.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public bool Contains(float x, float y)
		{
			return x >= this.Left && x <= this.Right && y >= this.Top && y <= this.Bottom;
		}

		/// <summary>
		/// Returns true if the given bounding box has the same position
		/// and size as this one.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(BoundingBoxF other)
		{
			return this.X == other.X && this.Y == other.Y && this.Width == other.Width && this.Height == other.Height;
		}

		/// <summary>
		/// Returns true if the given object is a bounding box with the
		/// same position and size as this one.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			return obj is BoundingBoxF other && this.Equals(other);
		}

		/// <summary>
		/// Returns a hash code for this bounding box, based on its
		/// properties.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = this.X.GetHashCode();
				hashCode = (hashCode * 397) ^ this.Y.GetHashCode();
				hashCode = (hashCode * 397) ^ this.Width.GetHashCode();
				hashCode = (hashCode * 397) ^ this.Height.GetHashCode();
				return hashCode;
			}
		}

		/// <summary>
		/// Returns true if the bounding boxes have the same position and
		/// size.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator ==(BoundingBoxF left, BoundingBoxF right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// Returns true if the bounding boxes do not have the same
		/// position and size.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator !=(BoundingBoxF left, BoundingBoxF right)
		{
			return !left.Equals(right);
		}

		/// <summary>
		/// Returns a string representation of the bounding box.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return $"X:{this.X}, Y:{this.Y}, Width:{this.Width}, Height:{this.Height}";
		}
	}
}
