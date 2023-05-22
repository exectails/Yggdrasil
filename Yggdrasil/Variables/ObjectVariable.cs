using System;
using System.IO;
using System.Xml.Serialization;

namespace Yggdrasil.Variables
{
	public partial class VariableContainer<TIdent>
	{
		/// <summary>
		/// A string type variable.
		/// </summary>
		public class ObjectVariable : IVariable<object>
		{
			private object _value;

			/// <summary>
			/// Returns the variable's underlying type.
			/// </summary>
			public VariableType Type => VariableType.Object;

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
			public object Value
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
			public ObjectVariable(TIdent ident, object value = null)
			{
				this.Ident = ident;
				this.Value = value;
			}

			/// <summary>
			/// Serializes the variable's value and returns it.
			/// </summary>
			/// <returns></returns>
			public string Serialize()
			{
				var serializer = new XmlSerializer(this.Value.GetType());

				using (var sw = new StringWriter())
				{
					serializer.Serialize(sw, this.Value);
					return sw.ToString();
				}
			}

			/// <summary>
			/// Reads the serialized value and sets it as the variable's
			/// value.
			/// </summary>
			/// <param name="value"></param>
			public void Deserialize(string value)
			{
				var serializer = new XmlSerializer(this.Value.GetType());

				using (var sr = new StringReader(value))
					this.Value = serializer.Deserialize(sr);
			}

			/// <summary>
			/// Returns a string representation of the variable's value.
			/// </summary>
			/// <returns></returns>
			public override string ToString() => this.Value.ToString();
		}
	}
}
