using System.Collections.Generic;
using System.Linq;

namespace Yggdrasil.Variables
{
	/// <summary>
	/// A collection of variables.
	/// </summary>
	/// <typeparam name="TIdent"></typeparam>
	public partial class VariableContainer<TIdent>
	{
		private readonly object _syncLock = new object();
		private readonly Dictionary<TIdent, IVariable> _vars = new Dictionary<TIdent, IVariable>();

		/// <summary>
		/// Gets or sets whether variables are created automatically on
		/// access if they don't exist yet.
		/// </summary>
		public bool AutoCreate { get; set; } = true;

		/// <summary>
		/// Adds the given variable to the container.
		/// </summary>
		/// <typeparam name="TVariable"></typeparam>
		/// <param name="variable"></param>
		/// <returns></returns>
		public TVariable Create<TVariable>(TVariable variable) where TVariable : IVariable
		{
			lock (_syncLock)
				_vars[variable.Ident] = variable;

			return variable;
		}

		/// <summary>
		/// Removes the variable with the given identifier from the
		/// container.
		/// </summary>
		/// <typeparam name="TVariable"></typeparam>
		/// <param name="ident"></param>
		/// <returns></returns>
		/// <exception cref="TypeMismatchException">
		/// Thrown when the type of the variable with the given identifier
		/// doesn't match the requested type.
		/// </exception>
		public TVariable Get<TVariable>(TIdent ident)
		{
			IVariable variable;

			lock (_syncLock)
			{
				if (!_vars.TryGetValue(ident, out variable))
					return default;
			}

			if (!(variable is TVariable tVariable))
				throw new TypeMismatchException($"Variable {ident} is not of type {typeof(TVariable)}");

			return tVariable;
		}

		/// <summary>
		/// Returns the variable with the given identifier, or null if
		/// no matching variable was found.
		/// </summary>
		/// <param name="ident"></param>
		/// <returns></returns>
		public IVariable Get(TIdent ident)
		{
			IVariable variable;

			lock (_syncLock)
			{
				if (!_vars.TryGetValue(ident, out variable))
					return default;
			}

			return variable;
		}

		/// <summary>
		/// Returns the variable with the given identifier via out,
		/// cast to the requested type. Returns false if no matching
		/// variable was found.
		/// </summary>
		/// <typeparam name="TVariable"></typeparam>
		/// <param name="ident"></param>
		/// <param name="variable"></param>
		/// <returns></returns>
		/// <exception cref="TypeMismatchException">
		/// Thrown when the type of the variable with the given identifier
		/// doesn't match the requested type.
		/// </exception>
		public bool TryGet<TVariable>(TIdent ident, out TVariable variable)
		{
			variable = this.Get<TVariable>(ident);
			return variable != null;
		}

		/// <summary>
		/// Returns the variable with the given identifier via out.
		/// Returns false if no matching variable was found.
		/// </summary>
		/// <param name="ident"></param>
		/// <param name="variable"></param>
		/// <returns></returns>
		/// <exception cref="TypeMismatchException">
		/// Thrown when the type of the variable with the given identifier
		/// doesn't match the requested type.
		/// </exception>
		public bool TryGet(TIdent ident, out IVariable variable)
		{
			variable = this.Get(ident);
			return variable != null;
		}

		/// <summary>
		/// Returns a list of all variables.
		/// </summary>
		/// <returns></returns>
		public IVariable[] GetAll()
		{
			lock (_syncLock)
				return _vars.Values.ToArray();
		}

		/// <summary>
		/// Returns true if a variable with the given identifier exists.
		/// </summary>
		/// <param name="ident"></param>
		/// <returns></returns>
		public bool Has(TIdent ident)
		{
			lock (_syncLock)
				return _vars.ContainsKey(ident);
		}
	}
}
