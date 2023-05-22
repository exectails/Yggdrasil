using System;

namespace Yggdrasil.Variables
{
	public partial class VariableContainer<TIdent>
	{
		/// <summary>
		/// A string type variable.
		/// </summary>
		public class StringVariable : IVariable<string>
		{
			private string _value;

			/// <summary>
			/// Returns the variable's underlying type.
			/// </summary>
			public VariableType Type => VariableType.String;

			/// <summary>
			/// Returns the variable's identifier.
			/// </summary>
			public TIdent Ident { get; }

			/// <summary>
			/// Fired when the variable's value changed.
			/// </summary>
			public event Action<IVariable> ValueChanged;

			/// <summary>
			/// Gets or sets the variable's value.
			/// </summary>
			public string Value
			{
				get => _value;
				set
				{
					if (_value == value)
						return;

					_value = value;
					this.ValueChanged?.Invoke(this);
				}
			}

			/// <summary>
			/// Creates new variable.
			/// </summary>
			/// <param name="ident"></param>
			/// <param name="value"></param>
			public StringVariable(TIdent ident, string value = null)
			{
				this.Ident = ident;
				this.Value = value;
			}

			/// <summary>
			/// Serializes the variable's value and returns it.
			/// </summary>
			/// <returns></returns>
			public string Serialize() => this.Value;

			/// <summary>
			/// Reads the serialized value and sets it as the variable's
			/// value.
			/// </summary>
			/// <param name="value"></param>
			public void Deserialize(string value) => this.Value = value;

			/// <summary>
			/// Returns a string representation of the variable's value.
			/// </summary>
			/// <returns></returns>
			public override string ToString() => this.Value;
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
		public StringVariable String(TIdent ident)
		{
			if (!this.TryGet<StringVariable>(ident, out var variable))
			{
				if (!this.AutoCreate)
					return null;

				variable = this.Create(new StringVariable(ident));
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
		public StringVariable String(TIdent ident, string value)
		{
			if (!this.TryGet<StringVariable>(ident, out var variable))
				return this.Create(new StringVariable(ident, value));

			variable.Value = value;

			return variable;
		}
	}
}
