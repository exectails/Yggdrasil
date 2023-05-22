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
			public event Action<TIdent> ValueChanged;

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
					this.ValueChanged?.Invoke(this.Ident);
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
	}
}
