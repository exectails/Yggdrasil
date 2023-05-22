namespace Yggdrasil.Variables
{
	/// <summary>
	/// The underlying type of a variable.
	/// </summary>
	public enum VariableType
	{
		/// <summary>
		/// A boolean type.
		/// </summary>
		Bool,

		/// <summary>
		/// An 8-bit unsigned integer.
		/// </summary>
		Byte,

		/// <summary>
		/// An 8-bit signed integer.
		/// </summary>
		SByte,

		/// <summary>
		/// A 16-bit signed integer.
		/// </summary>
		Short,

		/// <summary>
		/// A 16-bit unsigned integer.
		/// </summary>
		UShort,

		/// <summary>
		/// A 32-bit signed integer.
		/// </summary>
		Int,

		/// <summary>
		/// A 32-bit unsigned integer.
		/// </summary>
		UInt,

		/// <summary>
		/// A 64-bit signed integer.
		/// </summary>
		Long,

		/// <summary>
		/// A 64-bit unsigned integer.
		/// </summary>
		ULong,

		/// <summary>
		/// A 32-bit floating point number.
		/// </summary>
		Float,

		/// <summary>
		/// A 64-bit floating point number.
		/// </summary>
		Double,

		/// <summary>
		/// A string type.
		/// </summary>
		String,

		/// <summary>
		/// An object type.
		/// </summary>
		Object,
	}
}
