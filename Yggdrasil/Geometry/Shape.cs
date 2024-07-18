using System;

namespace Yggdrasil.Geometry
{
	/// <summary>
	/// A 2D shape.
	/// </summary>
	public interface IShape
	{
		/// <summary>
		/// Returns the center of the shape.
		/// </summary>
		Vector2 Center { get; }

		/// <summary>
		/// Returns true if the given position is within this shape.
		/// </summary>
		/// <param name="pos"></param>
		/// <returns></returns>
		bool IsInside(Vector2 pos);

		/// <summary>
		/// Returns the shape's edge points.
		/// </summary>
		/// <remarks>
		/// Depending on the shape the result of this method might be an
		/// aproximation, such as with circular shapes that don't
		/// technically have edges.
		/// </remarks>
		/// <returns></returns>
		Vector2[] GetEdgePoints();

		/// <summary>
		/// Returns the shape's outlines based on its edge points.
		/// </summary>
		/// <remarks>
		/// Most shapes have only one outline, but shapes can also be made
		/// out of multiple sub-shapes, in which case this will return
		/// outlines for all sub-shapes separately.
		/// </remarks>
		Outline[] GetOutlines();

		/// <summary>
		/// Returns a random point inside the shape.
		/// </summary>
		/// <param name="rnd"></param>
		/// <returns></returns>
		Vector2 GetRandomPoint(Random rnd);

		/// <summary>
		/// Moves shape to the given position and recalculates its properties.
		/// </summary>
		/// <param name="position"></param>
		void UpdatePosition(Vector2 position);
	}
}
