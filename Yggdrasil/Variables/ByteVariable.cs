using System;

namespace Yggdrasil.Variables
{
	public partial class VariableContainer<TIdent>
	{
		/// <summary>
		/// A byte type variable.
		/// </summary>
		public class ByteVariable : NumericVariable<int>, IFormattable
		{
			/// <summary>
			/// Returns underlying the type of this variable.
			/// </summary>
			public override VariableType Type => VariableType.Byte;

			/// <summary>
			/// Creates a new variable.
			/// </summary>
			/// <param name="ident"></param>
			/// <param name="value"></param>
			public ByteVariable(TIdent ident, byte value = 0) : this(ident, value, byte.MinValue, byte.MaxValue) { }

			/// <summary>
			/// Creates a new variable.
			/// </summary>
			/// <param name="ident"></param>
			/// <param name="value"></param>
			/// <param name="minValue"></param>
			/// <param name="maxValue"></param>
			public ByteVariable(TIdent ident, byte value, byte minValue, byte maxValue) : base(ident, value, minValue, maxValue) { }

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
			public override void Deserialize(string value) => this.Value = byte.Parse(value);
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
		public ByteVariable Byte(TIdent ident)
		{
			if (!this.TryGet<ByteVariable>(ident, out var variable))
			{
				if (!this.AutoCreate)
					return null;

				variable = this.Create(new ByteVariable(ident));
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
		public ByteVariable Byte(TIdent ident, byte value)
		{
			if (!this.TryGet<ByteVariable>(ident, out var variable))
				return this.Create(new ByteVariable(ident, value));

			variable.Value = value;

			return variable;
		}
	}
}
