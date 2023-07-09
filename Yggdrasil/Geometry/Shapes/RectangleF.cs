namespace Yggdrasil.Geometry.Shapes
{
	/// <summary>
	/// A rectangle shape.
	/// </summary>
	/// <remarks>
	/// To create a rotated rectangle use PolygonF.Rectangle.
	/// </remarks>
	public class RectangleF : PolygonF
	{
		/// <summary>
		/// Returns the rectangle's position.
		/// </summary>
		public Vector2F Position { get; }

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
			: base(pos, size.X, size.Y)
		{
			this.Position = pos;
			this.Size = size;
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
	}
}
