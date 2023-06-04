namespace Yggdrasil.Geometry
{
	/// <summary>
	/// Represents the outline of a shape.
	/// </summary>
	public readonly struct OutlineF
	{
		/// <summary>
		/// Returns the lines that make up the outline.
		/// </summary>
		public readonly LineF[] Lines;

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="lines"></param>
		public OutlineF(LineF[] lines)
		{
			this.Lines = lines;
		}
	}
}
