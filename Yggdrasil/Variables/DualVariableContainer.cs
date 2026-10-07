namespace Yggdrasil.Variables
{
	/// <summary>
	/// A wrapper around two commonly used variable containers: a permanent
	/// and a temporary one.
	/// </summary>
	public class DualVariableContainer<TIdent>
	{
		/// <summary>
		/// Returns the permanent variable container.
		/// </summary>
		/// <remarks>
		/// Permanent variables are typically stored in a database and are
		/// persisted across sessions.
		/// </remarks>
		public VariableContainer<TIdent> Permanent { get; } = new VariableContainer<TIdent>();

		/// <summary>
		/// Returns the temporary variable container.
		/// </summary>
		/// <remarks>
		/// Temporary variables are typically not stored in a database and
		/// are not persisted across sessions.
		/// </remarks>
		public VariableContainer<TIdent> Temporary { get; } = new VariableContainer<TIdent>();
	}
}
