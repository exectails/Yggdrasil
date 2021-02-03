using System;

namespace Yggdrasil.Util.Commands
{
	/// <summary>
	/// Generalized command holder
	/// </summary>
	/// <typeparam name="TFunc"></typeparam>
	public abstract class Command<TFunc> where TFunc : class
	{
		/// <summary>
		/// The command's name, which is used to call it.
		/// </summary>
		public string Name { get; protected set; }

		/// <summary>
		/// A description of how to use the command.
		/// </summary>
		/// <remarks>
		/// Recommended syntax:
		/// - &lt;parameter&gt;      Mandatory parameter.
		/// - [parameter]            Optional parameter.
		/// - parameter1|parameter2  Interchangeable parameters.
		/// </remarks>
		public string Usage { get; protected set; }

		/// <summary>
		/// A description of what the command does.
		/// </summary>
		public string Description { get; protected set; }

		/// <summary>
		/// Handler for the command.
		/// </summary>
		public TFunc Func { get; protected set; }

		/// <summary>
		/// Initializes Command.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="usage"></param>
		/// <param name="description"></param>
		/// <param name="func"></param>
		/// <exception cref="InvalidOperationException">
		/// Thrown if func is not a valid delegate.
		/// </exception>
		protected Command(string name, string usage, string description, TFunc func)
		{
			if (!typeof(TFunc).IsSubclassOf(typeof(Delegate)))
				throw new InvalidOperationException(typeof(TFunc).Name + " is not a delegate type");

			this.Name = name;
			this.Usage = usage;
			this.Description = description;
			this.Func = func;
		}
	}
}
