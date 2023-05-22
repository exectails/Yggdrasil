using System;

namespace Yggdrasil.Variables
{
	public partial class VariableContainer<TIdent>
	{
		/// <summary>
		/// An integer type variable.
		/// </summary>
		public class UIntVariable : NumericVariable<uint>, IFormattable
		{
			/// <summary>
			/// Returns underlying the type of this variable.
			/// </summary>
			public override VariableType Type => VariableType.UInt;

			/// <summary>
			/// Creates a new variable.
			/// </summary>
			/// <param name="ident"></param>
			/// <param name="value"></param>
			public UIntVariable(TIdent ident, uint value = 0) : this(ident, value, uint.MinValue, uint.MaxValue) { }

			/// <summary>
			/// Creates a new variable.
			/// </summary>
			/// <param name="ident"></param>
			/// <param name="value"></param>
			/// <param name="minValue"></param>
			/// <param name="maxValue"></param>
			public UIntVariable(TIdent ident, uint value, uint minValue, uint maxValue) : base(ident, value, minValue, maxValue) { }

			/// <summary>
			/// Seriealizes the variable's value and returns it.
			/// </summary>
			/// <returns></returns>
			public override string Serialize() => this.Value.ToString();

			/// <summary>
			/// Reads the serialized value and sets it as the variable's
			/// value.
			/// </summary>
			/// <param name="value"></param>
			public override void Deserialize(string value) => this.Value = uint.Parse(value);
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
		public UIntVariable UInt(TIdent ident)
		{
			if (!this.TryGet<UIntVariable>(ident, out var variable))
			{
				if (!this.AutoCreate)
					return null;

				variable = this.Create(new UIntVariable(ident));
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
		public UIntVariable UInt(TIdent ident, uint value)
		{
			if (!this.TryGet<UIntVariable>(ident, out var variable))
				return this.Create(new UIntVariable(ident, value));

			variable.Value = value;

			return variable;
		}
	}
}
