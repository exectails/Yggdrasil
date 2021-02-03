using System.Collections.Generic;

namespace Yggdrasil.Util.Commands
{
	/// <summary>
	/// Generalized command manager.
	/// </summary>
	/// <typeparam name="TCommand"></typeparam>
	/// <typeparam name="TFunc"></typeparam>
	public abstract class CommandManager<TCommand, TFunc>
		where TCommand : Command<TFunc>
		where TFunc : class
	{
		/// <summary>
		/// List of available commands.
		/// </summary>
		protected Dictionary<string, TCommand> _commands;

		/// <summary>
		/// Initializes manager.
		/// </summary>
		protected CommandManager()
		{
			_commands = new Dictionary<string, TCommand>();
		}

		/// <summary>
		/// Adds command to list of command handlers.
		/// </summary>
		/// <param name="command"></param>
		public void Add(TCommand command)
		{
			_commands[command.Name] = command;
		}

		/// <summary>
		/// Returns command or null if the command doesn't exist.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public TCommand GetCommand(string name)
		{
			_commands.TryGetValue(name, out var command);
			return command;
		}
	}
}
