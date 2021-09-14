using System;

namespace Yggdrasil.Parameters
{
	/// <summary>
	/// Describes a parameter of some object or entity.
	/// </summary>
	public interface IParameter
	{
		/// <summary>
		/// Returns the parameter's type.
		/// </summary>
		ParameterValueType ValueType { get; }

		/// <summary>
		/// Raised when the value of the parameter changes.
		/// </summary>
		event Action<IParameter> ValueChanged;
	}

	/// <summary>
	/// Specifies a parameter's value's type.
	/// </summary>
	public enum ParameterValueType
	{
		/// <summary>
		/// A byte value.
		/// </summary>
		Byte,

		/// <summary>
		/// A short value.
		/// </summary>
		Short,

		/// <summary>
		/// An int value.
		/// </summary>
		Int,

		/// <summary>
		/// A long value.
		/// </summary>
		Long,

		/// <summary>
		/// A float value.
		/// </summary>
		Float,

		/// <summary>
		/// A string value.
		/// </summary>
		String,
	}
}
