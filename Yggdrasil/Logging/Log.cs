using Yggdrasil.Logging.Targets;

namespace Yggdrasil.Logging
{
	/// <summary>
	/// Logs messages to command line and file.
	/// </summary>
	public static class Log
	{
		private static Logger GlobalLogger;

		/// <summary>
		/// Initiaizes global Log instance with the given name. If Init is
		/// not called, a default logger with the name of the assembly will
		/// be created.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static void Init(string name)
		{
			GlobalLogger = Logger.Get(name);

			GlobalLogger.AddTarget(new ConsoleTarget());
			GlobalLogger.AddTarget(new FileTarget("logs"));
		}

		/// <summary>
		/// Creates a logger if it doesn't exist yet.
		/// </summary>
		private static Logger GetLogger()
		{
			if (GlobalLogger == null)
				Init(null);

			return GlobalLogger;
		}

		/// <summary>
		/// Logs an info message.
		/// </summary>
		/// <param name="value"></param>
		public static void Info(string value) { GetLogger().Info(value); }

		/// <summary>
		/// Logs an info message.
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void Info(string format, params object[] args) { GetLogger().Info(format, args); }

		/// <summary>
		/// Logs an info message.
		/// </summary>
		/// <param name="obj"></param>
		public static void Info(object obj) { GetLogger().Info(obj); }

		/// <summary>
		/// Logs a warning message.
		/// </summary>
		/// <param name="value"></param>
		public static void Warning(string value) { GetLogger().Warning(value); }

		/// <summary>
		/// Logs a warning message.
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void Warning(string format, params object[] args) { GetLogger().Warning(format, args); }

		/// <summary>
		/// Logs a warning message.
		/// </summary>
		/// <param name="obj"></param>
		public static void Warning(object obj) { GetLogger().Warning(obj); }

		/// <summary>
		/// Logs an error message.
		/// </summary>
		/// <param name="value"></param>
		public static void Error(string value) { GetLogger().Error(value); }

		/// <summary>
		/// Logs an error message.
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void Error(string format, params object[] args) { GetLogger().Error(format, args); }

		/// <summary>
		/// Logs an error message.
		/// </summary>
		/// <param name="obj"></param>
		public static void Error(object obj) { GetLogger().Error(obj); }

		/// <summary>
		/// Logs a debug message.
		/// </summary>
		/// <param name="value"></param>
		public static void Debug(string value) { GetLogger().Debug(value); }

		/// <summary>
		/// Logs a debug message.
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void Debug(string format, params object[] args) { GetLogger().Debug(format, args); }

		/// <summary>
		/// Logs a debug message.
		/// </summary>
		/// <param name="obj"></param>
		public static void Debug(object obj) { GetLogger().Debug(obj); }

		/// <summary>
		/// Logs a status message.
		/// </summary>
		/// <param name="value"></param>
		public static void Status(string value) { GetLogger().Status(value); }

		/// <summary>
		/// Logs a status message.
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void Status(string format, params object[] args) { GetLogger().Status(format, args); }

		/// <summary>
		/// Logs a status message.
		/// </summary>
		/// <param name="obj"></param>
		public static void Status(object obj) { GetLogger().Status(obj); }

		/// <summary>
		/// Sets levels that should not be logged.
		/// </summary>
		/// <param name="levels"></param>
		public static void SetFilter(LogLevel levels)
		{
			var targets = GetLogger().GetTargets();

			foreach (var target in targets)
				target.Filter = levels;
		}
	}
}
