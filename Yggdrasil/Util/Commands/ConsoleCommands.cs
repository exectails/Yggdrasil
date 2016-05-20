// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Yggdrasil.Logging;

namespace Yggdrasil.Util.Commands
{
	/// <summary>
	/// Console command manager.
	/// </summary>
	public class ConsoleCommands : CommandManager<ConsoleCommand, ConsoleCommandFunc>
	{
		/// <summary>
		/// Creates new instance of ConsoleCommands and adds the help, exit,
		/// and status commands.
		/// </summary>
		public ConsoleCommands()
		{
			_commands = new Dictionary<string, ConsoleCommand>();

			this.Add("help", "Displays this help", HandleHelp);
			this.Add("exit", "Closes application/server", HandleExit);
			this.Add("status", "Displays application status", HandleStatus);
		}

		/// <summary>
		/// Adds new command handler.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="description"></param>
		/// <param name="handler"></param>
		public void Add(string name, string description, ConsoleCommandFunc handler)
		{
			this.Add(name, "", description, handler);
		}

		/// <summary>
		/// Adds new command handler.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="usage"></param>
		/// <param name="description"></param>
		/// <param name="handler"></param>
		public void Add(string name, string usage, string description, ConsoleCommandFunc handler)
		{
			_commands[name] = new ConsoleCommand(name, usage, description, handler);
		}

		/// <summary>
		/// Waits and parses commands till "exit" is entered.
		/// </summary>
		public void Wait()
		{
			// Just wait if not running in a console
			if (!CliUtil.IsUserInteractive)
			{
				var reset = new ManualResetEvent(false);
				reset.WaitOne();
				return;
			}

			Log.Info("Type 'help' for a list of console commands.");

			while (true)
			{
				var line = Console.ReadLine();

				var args = this.ParseLine(line);
				if (args.Count == 0)
					continue;

				var command = this.GetCommand(args[0]);
				if (command == null)
				{
					Log.Info("Unknown command '{0}'", args[0]);
					continue;
				}

				var result = command.Func(line, args);
				if (result == CommandResult.Break)
				{
					break;
				}
				else if (result == CommandResult.Fail)
				{
					Log.Error("Failed to run command '{0}'.", command.Name);
				}
				else if (result == CommandResult.InvalidArgument)
				{
					Log.Info("Usage: {0} {1}", command.Name, command.Usage);
				}
			}
		}

		/// <summary>
		/// Handles help command, listing all available console commands.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		protected virtual CommandResult HandleHelp(string command, IList<string> args)
		{
			var maxLength = _commands.Values.Max(a => a.Name.Length);

			Log.Info("Available commands");
			foreach (var cmd in _commands.Values.OrderBy(a => a.Name))
				Log.Info("  {0,-" + (maxLength + 2) + "}{1}", cmd.Name, cmd.Description);

			return CommandResult.Okay;
		}

		/// <summary>
		/// Handles status command, displaying information about
		/// the application, like current memory usage.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		protected virtual CommandResult HandleStatus(string command, IList<string> args)
		{
			Log.Status("Memory Usage: {0:N0} KB", Math.Round(GC.GetTotalMemory(false) / 1024f));

			return CommandResult.Okay;
		}

		/// <summary>
		/// Handles exist command, closing the application immediately.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		protected virtual CommandResult HandleExit(string command, IList<string> args)
		{
			CliUtil.Exit(0, false);

			return CommandResult.Okay;
		}
	}

	/// <summary>
	/// Represents a command for the ConsoleCommands class.
	/// </summary>
	public class ConsoleCommand : Command<ConsoleCommandFunc>
	{
		public ConsoleCommand(string name, string usage, string description, ConsoleCommandFunc func)
			: base(name, usage, description, func)
		{
		}
	}

	/// <summary>
	/// Represents a command handler for the ConsoleCommands class.
	/// </summary>
	/// <param name="command"></param>
	/// <param name="args"></param>
	/// <returns></returns>
	public delegate CommandResult ConsoleCommandFunc(string command, IList<string> args);

	/// <summary>
	/// Command results for the Console Commands.
	/// </summary>
	public enum CommandResult { Okay, Fail, InvalidArgument, Break }
}
