using System;

namespace Yggdrasil.Geometry
{
	/// <summary>
	/// Represents a 2-dimensional vector.
	/// </summary>
	public readonly struct Vector2
	{
		/// <summary>
		/// The vector's X value.
		/// </summary>
		public readonly int X;

		/// <summary>
		/// The vector's Y value.
		/// </summary>
		public readonly int Y;

		/// <summary>
		/// Returns a new instance with all values set to 0.
		/// </summary>
		public static Vector2 Zero => new Vector2(0, 0);

		/// <summary>
		/// Creates new vector.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public Vector2(int x, int y)
		{
			this.X = x;
			this.Y = y;
		}

		/// <summary>
		/// Returns distance between this and another vector in 2D space.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public double GetDistance(Vector2 other)
		{
			return Math.Sqrt(Math.Pow(this.X - other.X, 2) + Math.Pow(this.Y - other.Y, 2));
		}

		/// <summary>
		/// Returns direction the other vetcor is in as an angle in degree.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public double GetAngle(Vector2 other)
		{
			var radianAngle = Math.Atan2(other.Y - this.Y, other.X - this.X);

			var result = radianAngle * (180.0 / Math.PI) - 90.0;

			// Normalize
			result %= 360.0;
			result = (result > 0 ? result : result + 360);

			if (Math.Abs(result - 360) < 0.00001)
				result = 0;

			return result;
		}

		/// <summary>
		/// Returns a new vector, that had vector2's values added
		/// to vector1's values.
		/// </summary>
		/// <param name="vector1"></param>
		/// <param name="vector2"></param>
		/// <returns></returns>
		public static Vector2 operator +(Vector2 vector1, Vector2 vector2)
		{
			return new Vector2(vector2.X + vector1.X, vector2.Y + vector1.Y);
		}

		/// <summary>
		/// Returns a new vector, that had vector2's values subtracted
		/// from vector1's values.
		/// </summary>
		/// <param name="vector1"></param>
		/// <param name="vector2"></param>
		/// <returns></returns>
		public static Vector2 operator -(Vector2 vector1, Vector2 vector2)
		{
			return new Vector2(vector1.X - vector2.X, vector1.Y - vector2.Y);
		}

		/// <summary>
		/// Returns a new vector, that had vector1's values multiplied
		/// by the given amount.
		/// </summary>
		/// <param name="vector"></param>
		/// <param name="multiplier"></param>
		/// <returns></returns>
		public static Vector2 operator *(Vector2 vector, int multiplier)
		{
			return new Vector2(vector.X * multiplier, vector.Y * multiplier);
		}

		/// <summary>
		/// Returns a new vector, that had vector1's values divided
		/// by the given amount.
		/// </summary>
		/// <param name="vector1"></param>
		/// <param name="divider"></param>
		/// <returns></returns>
		public static Vector2 operator /(Vector2 vector1, int divider)
		{
			return new Vector2(vector1.X / divider, vector1.Y / divider);
		}

		/// <summary>
		/// Returns true if the vector's coordinates are the same.
		/// </summary>
		/// <param name="vector1"></param>
		/// <param name="vector2"></param>
		/// <returns></returns>
		public static bool operator ==(Vector2 vector1, Vector2 vector2)
		{
			return (vector1.X == vector2.X && vector1.Y == vector2.Y);
		}

		/// <summary>
		/// Returns true if the vector's coordinates are not the same.
		/// </summary>
		/// <param name="vector1"></param>
		/// <param name="vector2"></param>
		/// <returns></returns>
		public static bool operator !=(Vector2 vector1, Vector2 vector2)
		{
			return !(vector1 == vector2);
		}

		/// <summary>
		/// Returns a hash code for this vector that will equal vector
		/// with the same coordinates.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return this.X.GetHashCode() ^ this.Y.GetHashCode();
		}

		/// <summary>
		/// Returns true if the given object is this exact vector instance.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			return obj is Vector2 vector && this == vector;
		}

		/// <summary>
		/// Returns a string representation of this Vector2.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format("(Vector2 - X: {0:n2}, Y: {1:n2})", this.X, this.Y);
		}
	}
}
