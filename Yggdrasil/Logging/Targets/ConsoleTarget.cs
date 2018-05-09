// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Yggdrasil.Logging.Targets
{
	/// <summary>
	/// Logger target logging to the Console standard output.
	/// </summary>
	/// <remarks>
	/// Recognizes codes c[0-9]*, b[0-9]*, and r, for color, background
	/// color, and resetting the colors respectively.
	/// </remarks>
	public class ConsoleTarget : LoggerTarget
	{
		private readonly ConsoleColor _defaultForeground = Console.ForegroundColor;
		private readonly ConsoleColor _defaultBackground = Console.BackgroundColor;

		private const int CodeMaxLength = 6;
		private readonly static Regex CodeRegex = new Regex("\u001B" + @"\^(?<command>[a-z])(?<arg>[0-9]{1,2})?;", RegexOptions.Compiled);

		/// <summary>
		/// Writes message to Console standard output.
		/// </summary>
		/// <param name="level"></param>
		/// <param name="message"></param>
		/// <param name="messageRaw"></param>
		/// <param name="messageClean"></param>
		public override void Write(LogLevel level, string message, string messageRaw, string messageClean)
		{
			// A little faster than writing every single char
			using (var stream = new StreamWriter(Console.OpenStandardOutput()))
			{
				for (var i = 0; i < messageRaw.Length; ++i)
				{
					if (messageRaw[i] == '\x1B')
					{
						try
						{
							var match = CodeRegex.Match(messageRaw, i, CodeMaxLength);
							if (match.Success)
							{
								var command = match.Groups["command"].Value[0];
								var arg = match.Groups["arg"].Value;

								stream.Flush();

								switch (command)
								{
									case 'c': // Color
										Console.ForegroundColor = (arg != "" ? (ConsoleColor)int.Parse(arg) : _defaultForeground);
										break;
									case 'b': // Background
										Console.BackgroundColor = (arg != "" ? (ConsoleColor)int.Parse(arg) : _defaultBackground);
										break;
									case 'r': // Reset
										Console.ResetColor();
										break;
								}

								i += match.Length - 1;

								continue;
							}
						}
						catch
						{
							// Continue to write message if anything went
							// wrong, it's more important to continue logging
							// than to worry about codes.
						}
					}

					stream.Write(messageRaw[i]);
				}
			}
		}

		/// <summary>
		/// Returns color coded formats, based on log level.
		/// </summary>
		/// <param name="level"></param>
		/// <returns></returns>
		public override string GetFormat(LogLevel level)
		{
			switch (level)
			{
				case LogLevel.Info: return "\u001B^c15;[{0}]\u001B^r; - {1}"; // White
				case LogLevel.Warning: return "\u001B^c14;[{0}]\u001B^r; - {1}"; // Yellow
				case LogLevel.Error: return "\u001B^c12;[{0}]\u001B^r; - {1}"; // Red
				case LogLevel.Debug: return "\u001B^c8;[{0}]\u001B^r; - {1}"; // Dark Gray
				case LogLevel.Status: return "\u001B^c10;[{0}]\u001B^r; - {1}"; // Green
			}

			return "[{0}] - {1}";
		}
	}
}
