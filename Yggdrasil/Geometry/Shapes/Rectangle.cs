namespace Yggdrasil.Geometry.Shapes
{
	/// <summary>
	/// A rectangle shape.
	/// </summary>
	/// <remarks>
	/// To create a rotated rectangle use Polygon.Rectangle.
	/// </remarks>
	public class Rectangle : Polygon
	{
		/// <summary>
		/// Returns the rectangle's position.
		/// </summary>
		public Vector2 Position { get; }

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
			: base(pos, size.X, size.Y)
		{
			this.Position = pos + this.Size / 2;
			this.Size = size;
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
	}
}
