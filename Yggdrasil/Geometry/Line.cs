namespace Yggdrasil.Geometry
{
	/// <summary>
	/// A line, respresent by two points.
	/// </summary>
	public readonly struct Line
	{
		/// <summary>
		/// The first point of the line.
		/// </summary>
		public readonly Vector2 Point1;

		/// <summary>
		/// The second point of the line.
		/// </summary>
		public readonly Vector2 Point2;

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="point1"></param>
		/// <param name="point2"></param>
		public Line(Vector2 point1, Vector2 point2)
		{
			this.Point1 = point1;
			this.Point2 = point2;
		}

		/// <summary>
		/// Returns true if this line intersects with the given line,
		/// and returns the point at which they intersect via out.
		/// </summary>
		/// <param name="otherLine"></param>
		/// <param name="intersection"></param>
		/// <returns></returns>
		public bool Intersects(Line otherLine, out Vector2 intersection)
		{
			intersection = Vector2.Zero;

			double x1 = this.Point1.X;
			double y1 = this.Point1.Y;
			double x2 = this.Point2.X;
			double y2 = this.Point2.Y;
			double x3 = otherLine.Point1.X;
			double y3 = otherLine.Point1.Y;
			double x4 = otherLine.Point2.X;
			double y4 = otherLine.Point2.Y;

			var denom = ((x2 - x1) * (y4 - y3)) - ((y2 - y1) * (x4 - x3));

			// Parallel 
			if (denom == 0)
				return false;

			var numer = ((y1 - y3) * (x4 - x3)) - ((x1 - x3) * (y4 - y3));
			var r = numer / denom;
			var numer2 = ((y1 - y3) * (x2 - x1)) - ((x1 - x3) * (y2 - y1));
			var s = numer2 / denom;

			// No intersection
			if ((r < 0 || r > 1) || (s < 0 || s > 1))
				return false;

			var interX = x1 + (r * (x2 - x1));
			var interY = y1 + (r * (y2 - y1));

			intersection = new Vector2((int)interX, (int)interY);

			return true;
		}

		/// <summary>
		/// Returns true if the line's points are the same.
		/// </summary>
		/// <param name="val1"></param>
		/// <param name="val2"></param>
		/// <returns></returns>
		public static bool operator ==(Line val1, Line val2)
		{
			return (val1.Point1 == val2.Point1 && val1.Point2 == val2.Point2);
		}

		/// <summary>
		/// Returns true if the line's points are not the same.
		/// </summary>
		/// <param name="val1"></param>
		/// <param name="val2"></param>
		/// <returns></returns>
		public static bool operator !=(Line val1, Line val2)
		{
			return !(val1 == val2);
		}

		/// <summary>
		/// Returns a hash code for this line that will equal lines
		/// with the same points.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return this.Point1.GetHashCode() ^ this.Point2.GetHashCode();
		}

		/// <summary>
		/// Returns true if the given object is this exact line instance.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			return obj is Line && this == (Line)obj;
		}

		/// <summary>
		/// Returns a string representation of this Line.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format("(Line - Point1: {0}/{1}, Point2: {2}/{3})", this.Point1.X, this.Point1.Y, this.Point2.X, this.Point2.Y);
		}
	}
}
