using System;

namespace Yggdrasil.Variables
{
	public partial class VariableContainer<TIdent>
	{
		/// <summary>
		/// Describes a variable.
		/// </summary>
		public interface IVariable
		{
			/// <summary>
			/// Returns the variable's underlying type.
			/// </summary>
			VariableType Type { get; }

			/// <summary>
			/// Returns the variable's identifier.
			/// </summary>
			TIdent Ident { get; }

			/// <summary>
			/// Fired when the variable's value changed.
			/// </summary>
			event Action<IVariable> ValueChanged;

			/// <summary>
			/// Serializes the variable's value and returns it.
			/// </summary>
			/// <returns></returns>
			string Serialize();

			/// <summary>
			/// Reads the given value and sets it as the variable's
			/// value.
			/// </summary>
			/// <param name="value"></param>
			void Deserialize(string value);
		}

		/// <summary>
		/// Descibes a variable with a value.
		/// </summary>
		/// <typeparam name="TValue"></typeparam>
		public interface IVariable<TValue> : IVariable
		{
			/// <summary>
			/// Gets or sets the variable's value.
			/// </summary>
			TValue Value { get; set; }
		}
	}
}
