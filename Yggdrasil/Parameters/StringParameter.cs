using System;

namespace Yggdrasil.Parameters
{
	/// <summary>
	/// Represents a parameter that contains a string.
	/// </summary>
	public class StringParameter : IParameter
	{
		private string _value;

		/// <summary>
		/// Returns the parameters type (String).
		/// </summary>
		public ParameterValueType ValueType => ParameterValueType.String;

		/// <summary>
		/// Raised when the parameter's value changed.
		/// </summary>
		public event Action<IParameter> ValueChanged;

		/// <summary>
		/// Returns the parameter's value.
		/// </summary>
		public string Value
		{
			get => _value;
			set
			{
				var changed = (value != _value);
				_value = value;

				if (changed)
					this.ValueChanged?.Invoke(this);
			}
		}
	}
}
