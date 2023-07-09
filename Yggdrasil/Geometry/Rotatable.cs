namespace Yggdrasil.Geometry
{
	/// <summary>
	/// A shape that can be rotated.
	/// </summary>
	public interface IRotatableF
	{
		/// <summary>
		/// Rotates the shape by the given angle around its center.
		/// </summary>
		/// <param name="degreeAngle"></param>
		void Rotate(float degreeAngle);

		/// <summary>
		/// Rotates the shape by the given angle around the given pivot point.
		/// </summary>
		/// <param name="pivot"></param>
		/// <param name="degreeAngle"></param>
		void RotateAround(Vector2F pivot, float degreeAngle);
	}
}
