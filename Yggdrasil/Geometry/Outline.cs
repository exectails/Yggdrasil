namespace Yggdrasil.Geometry
{
	/// <summary>
	/// Represents the outline of a shape.
	/// </summary>
	public readonly struct Outline
	{
		/// <summary>
		/// Returns the lines that make up the outline.
		/// </summary>
		public readonly Line[] Lines;

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="lines"></param>
		public Outline(Line[] lines)
		{
			this.Lines = lines;
		}
	}
}
