using System;

namespace Yggdrasil.Variables
{
	public partial class VariableContainer<TIdent>
	{
		/// <summary>
		/// A boolean type variable.
		/// </summary>
		public class BoolVariable : IVariable<bool>
		{
			private bool _value;

			/// <summary>
			/// Returns the variable's underlying type.
			/// </summary>
			public VariableType Type => VariableType.Bool;

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
			public virtual bool Value
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
			public BoolVariable(TIdent ident, bool value = false)
			{
				this.Ident = ident;
				this.Value = value;
			}

			/// <summary>
			/// Toggles the variable's value and returns the new value.
			/// </summary>
			/// <returns></returns>
			public bool Toggle()
			{
				this.Value = !this.Value;
				return this.Value;
			}

			/// <summary>
			/// Sets the variable's value to true if it is currently false
			/// and returns whether it was set to true. If it was already
			/// true, it will remain true and false will be returned.
			/// </summary>
			/// <returns></returns>
			public bool EnableOnce()
			{
				if (this.Value)
					return false;

				this.Value = true;
				return true;
			}

			/// <summary>
			/// Serializes the variable's value and returns it.
			/// </summary>
			/// <returns></returns>
			public string Serialize() => this.Value.ToString();

			/// <summary>
			/// Reads the serialized value and sets it as the variable's
			/// value.
			/// </summary>
			/// <param name="value"></param>
			public void Deserialize(string value) => this.Value = string.Compare(value, "true", StringComparison.OrdinalIgnoreCase) == 0;

			/// <summary>
			/// Returns a string representation of the variable's value.
			/// </summary>
			/// <returns></returns>
			public override string ToString() => this.Value.ToString();
		}
	}
}
