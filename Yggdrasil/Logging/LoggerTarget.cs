// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Yggdrasil.Logging
{
	public abstract class LoggerTarget
	{
		public LogLevel Filter { get; set; }
		public Logger Logger { get; internal set; }

		public abstract void Write(LogLevel level, string message, string messageRaw, string messageClean);

		public abstract string GetFormat(LogLevel level);

		public bool Filtered(LogLevel level)
		{
			return ((Filter & level) != 0);
		}
	}
}
