namespace Yggdrasil.Variables.DefaultGetters
{
	/// <summary>
	/// Extensions for <see cref="VariableContainer{TIdent}"/> to provide
	/// accessor methods for the default variable types.
	/// </summary>
	public static partial class VariableContainerExtensions
	{
		/// <summary>
		/// Returns the variable with the given identifier.
		/// </summary>
		/// <typeparam name="TIdent"></typeparam>
		/// <param name="container"></param>
		/// <param name="ident"></param>
		/// <returns></returns>
		/// <exception cref="TypeMismatchException">
		/// Thrown when the variable with the given identifier is of a
		/// different type.
		/// </exception>
		public static VariableContainer<TIdent>.ByteVariable Byte<TIdent>(this VariableContainer<TIdent> container, TIdent ident)
		{
			if (!container.TryGet<VariableContainer<TIdent>.ByteVariable>(ident, out var variable))
			{
				if (!container.AutoCreate)
					return null;

				variable = container.Create(new VariableContainer<TIdent>.ByteVariable(ident));
			}

			return variable;
		}

		/// <summary>
		/// Sets the value of the variable with the given identifier.
		/// If the variable doesn't exist yet, it will be created.
		/// </summary>
		/// <typeparam name="TIdent"></typeparam>
		/// <param name="container"></param>
		/// <param name="ident"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		/// <exception cref="TypeMismatchException">
		/// Thrown when the variable with the given identifier is of a
		/// different type.
		/// </exception>
		public static VariableContainer<TIdent>.ByteVariable Byte<TIdent>(this VariableContainer<TIdent> container, TIdent ident, byte value)
		{
			if (!container.TryGet<VariableContainer<TIdent>.ByteVariable>(ident, out var variable))
				return container.Create(new VariableContainer<TIdent>.ByteVariable(ident, value));

			variable.Value = value;

			return variable;
		}
	}
}
