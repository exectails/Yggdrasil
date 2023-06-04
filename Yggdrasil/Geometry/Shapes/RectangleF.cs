using System;
using Yggdrasil.Extensions;

namespace Yggdrasil.Geometry.Shapes
{
	/// <summary>
	/// A rectangle shape.
	/// </summary>
	/// <remarks>
	/// To create a rotated rectangle use PolygonF.Rectangle.
	/// </remarks>
	public class RectangleF : IShapeF
	{
		private Vector2F[] _edgePoints;
		private OutlineF[] _outlines;

		/// <summary>
		/// Returns the rectangle's position.
		/// </summary>
		public Vector2F Position { get; }

		/// <summary>
		/// Returns the rectangle's center position.
		/// </summary>
		public Vector2F Center { get; }

		/// <summary>
		/// Returns the rectangle's size.
		/// </summary>
		public Vector2F Size { get; }

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="pos"></param>
		/// <param name="size"></param>
		public RectangleF(Vector2F pos, Vector2F size)
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
		public static RectangleF Centered(Vector2F center, Vector2F size)
		{
			var topLeft = center - (size / 2);
			return new RectangleF(topLeft, size);
		}

		/// <summary>
		/// Returns true if the given position is within this rectangle.
		/// </summary>
		/// <param name="pos"></param>
		/// <returns></returns>
		public bool IsInside(Vector2F pos)
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
		public Vector2F[] GetEdgePoints()
		{
			if (_edgePoints == null)
			{
				_edgePoints = new Vector2F[4]
				{
					new Vector2F(this.Center.X - this.Size.X / 2, this.Center.Y - this.Size.Y / 2),
					new Vector2F(this.Center.X + this.Size.X / 2, this.Center.Y - this.Size.Y / 2),
					new Vector2F(this.Center.X + this.Size.X / 2, this.Center.Y + this.Size.Y / 2),
					new Vector2F(this.Center.X - this.Size.X / 2, this.Center.Y + this.Size.Y / 2),
				};
			}

			return _edgePoints;
		}

		/// <summary>
		/// Returns the rectangle's outline based on its edge points.
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
		/// Returns random point inside the rectangle.
		/// </summary>
		/// <param name="rnd"></param>
		/// <returns></returns>
		public Vector2F GetRandomPoint(Random rnd)
		{
			var x = rnd.Between(this.Center.X - this.Size.X / 2, this.Center.X + this.Size.X / 2);
			var y = rnd.Between(this.Center.Y - this.Size.Y / 2, this.Center.Y + this.Size.Y / 2);

			return new Vector2F(x, y);
		}
	}
}
