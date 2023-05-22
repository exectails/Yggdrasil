using System;
using System.Globalization;

namespace Yggdrasil.Variables
{
	public partial class VariableContainer<TIdent>
	{
		/// <summary>
		/// A float type variable.
		/// </summary>
		public class FloatVariable : NumericVariable<float>, IFormattable
		{
			/// <summary>
			/// Returns underlying the type of this variable.
			/// </summary>
			public override VariableType Type => VariableType.Float;

			/// <summary>
			/// Creates a new variable.
			/// </summary>
			/// <param name="ident"></param>
			/// <param name="value"></param>
			public FloatVariable(TIdent ident, float value = 0) : this(ident, value, float.MinValue, float.MaxValue) { }

			/// <summary>
			/// Creates a new variable.
			/// </summary>
			/// <param name="ident"></param>
			/// <param name="value"></param>
			/// <param name="minValue"></param>
			/// <param name="maxValue"></param>
			public FloatVariable(TIdent ident, float value, float minValue, float maxValue) : base(ident, value, minValue, maxValue) { }

			/// <summary>
			/// Seriealizes the variable's value and returns it.
			/// </summary>
			/// <returns></returns>
			public override string Serialize() => this.Value.ToString("g", CultureInfo.InvariantCulture);

			/// <summary>
			/// Reads the serialized value and sets it as the variable's
			/// value.
			/// </summary>
			/// <param name="value"></param>
			public override void Deserialize(string value) => this.Value = float.Parse(value, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Returns the variable with the given identifier.
		/// </summary>
		/// <param name="ident"></param>
		/// <returns></returns>
		/// <exception cref="TypeMismatchException">
		/// Thrown when the variable with the given identifier is of a
		/// different type.
		/// </exception>
		public FloatVariable Float(TIdent ident)
		{
			if (!this.TryGet<FloatVariable>(ident, out var variable))
			{
				if (!this.AutoCreate)
					return null;

				variable = this.Create(new FloatVariable(ident));
			}

			return variable;
		}

		/// <summary>
		/// Sets the value of the variable with the given identifier.
		/// If the variable doesn't exist yet, it will be created.
		/// </summary>
		/// <param name="ident"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		/// <exception cref="TypeMismatchException">
		/// Thrown when the variable with the given identifier is of a
		/// different type.
		/// </exception>
		public FloatVariable Float(TIdent ident, float value)
		{
			if (!this.TryGet<FloatVariable>(ident, out var variable))
				return this.Create(new FloatVariable(ident, value));

			variable.Value = value;

			return variable;
		}
	}
}
