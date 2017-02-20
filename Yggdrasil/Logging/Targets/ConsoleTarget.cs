// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.IO;

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
		private ConsoleColor DefaultForeground = Console.ForegroundColor;
		private ConsoleColor DefaultBackground = Console.BackgroundColor;

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
				for (int i = 0; i < messageRaw.Length; ++i)
				{
					if (messageRaw[i] == '^')
					{
						// Flush, so we get the color updates
						stream.Flush();

						var command = messageRaw[i + 1];
						var end = messageRaw.IndexOf(';', i);
						if (end != -1)
						{
							var value = messageRaw.Substring(i + 2, end - i - 2);

							switch (command)
							{
								case 'c': // Color
									Console.ForegroundColor = (value != "" ? (ConsoleColor)int.Parse(value) : DefaultForeground);
									break;
								case 'b': // Background
									Console.BackgroundColor = (value != "" ? (ConsoleColor)int.Parse(value) : DefaultBackground);
									break;
								case 'r': // Reset
									Console.ResetColor();
									break;
							}

							i = end;

							continue;
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
				case LogLevel.Info: return "^c15;[{0}]^r; - {1}"; // White
				case LogLevel.Warning: return "^c14;[{0}]^r; - {1}"; // Yellow
				case LogLevel.Error: return "^c12;[{0}]^r; - {1}"; // Red
				case LogLevel.Debug: return "^c8;[{0}]^r; - {1}"; // Dark Gray
				case LogLevel.Status: return "^c10;[{0}]^r; - {1}"; // Green
			}

			return "[{0}] - {1}";
		}
	}
}
