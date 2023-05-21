using System;

namespace Yggdrasil.Geometry.Shapes
{
	/// <summary>
	/// A rectangle shape.
	/// </summary>
	/// <remarks>
	/// To create a rotated rectangle use Polygon.Rectangle.
	/// </remarks>
	public class Rectangle : IShape
	{
		private Vector2[] _edgePoints;
		private Outline[] _outlines;

		/// <summary>
		/// Returns the rectangle's position.
		/// </summary>
		public Vector2 Position { get; }

		/// <summary>
		/// Returns the rectangle's center position.
		/// </summary>
		public Vector2 Center { get; }

		/// <summary>
		/// Returns the rectangle's size.
		/// </summary>
		public Vector2 Size { get; }

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="pos"></param>
		/// <param name="size"></param>
		public Rectangle(Vector2 pos, Vector2 size)
		{
			this.Position = pos;
			this.Size = size;

			this.Center = this.Position + this.Size / 2;
		}

		/// <summary>
		/// Returns a new rectangle from a center position and a size.
		/// </summary>
		/// <param name="center"></param>
		/// <param name="size"></param>
		/// <returns></returns>
		public static Rectangle Centered(Vector2 center, Vector2 size)
		{
			var topLeft = center - (size / 2);
			return new Rectangle(topLeft, size);
		}

		/// <summary>
		/// Returns true if the given position is within this rectangle.
		/// </summary>
		/// <param name="pos"></param>
		/// <returns></returns>
		public bool IsInside(Vector2 pos)
		{
			if (pos.X < this.Center.X - this.Size.X / 2 || pos.X > this.Center.X + this.Size.X / 2)
				return false;

			if (pos.Y < this.Center.Y - this.Size.Y / 2 || pos.Y > this.Center.Y + this.Size.Y / 2)
				return false;

			return true;
		}

		/// <summary>
		/// Returns the rectangle's four edge points.
		/// </summary>
		/// <returns></returns>
		public Vector2[] GetEdgePoints()
		{
			if (_edgePoints == null)
			{
				_edgePoints = new Vector2[4]
				{
					new Vector2(this.Center.X - this.Size.X / 2, this.Center.Y - this.Size.Y / 2),
					new Vector2(this.Center.X + this.Size.X / 2, this.Center.Y - this.Size.Y / 2),
					new Vector2(this.Center.X + this.Size.X / 2, this.Center.Y + this.Size.Y / 2),
					new Vector2(this.Center.X - this.Size.X / 2, this.Center.Y + this.Size.Y / 2),
				};
			}

			return _edgePoints;
		}

		/// <summary>
		/// Returns the rectangle's outline based on its edge points.
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
		/// Returns random point inside the rectangle.
		/// </summary>
		/// <param name="rnd"></param>
		/// <returns></returns>
		public Vector2 GetRandomPoint(Random rnd)
		{
			var x = rnd.Next(this.Center.X - this.Size.X / 2, this.Center.X + this.Size.X / 2 + 1);
			var y = rnd.Next(this.Center.Y - this.Size.Y / 2, this.Center.Y + this.Size.Y / 2 + 1);

			return new Vector2(x, y);
		}
	}
}
