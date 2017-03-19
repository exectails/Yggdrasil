// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.IO;
using Yggdrasil.Logging.Targets;

namespace Yggdrasil.Logging
{
	/// <summary>
	/// Logs messages to command line and file.
	/// </summary>
	public static class Log
	{
		private static Logger _logger = Logger.Get();

		static Log()
		{
			_logger.AddTarget(new ConsoleTarget());
			_logger.AddTarget(new FileTarget("logs"));
		}

		public static void Info(string value) { _logger.Info(value); }
		public static void Info(string format, params object[] args) { _logger.Info(format, args); }
		public static void Info(object obj) { _logger.Info(obj); }

		public static void Warning(string value) { _logger.Warning(value); }
		public static void Warning(string format, params object[] args) { _logger.Warning(format, args); }
		public static void Warning(object obj) { _logger.Warning(obj); }

		public static void Error(string value) { _logger.Error(value); }
		public static void Error(string format, params object[] args) { _logger.Error(format, args); }
		public static void Error(object obj) { _logger.Error(obj); }

		public static void Debug(string value) { _logger.Debug(value); }
		public static void Debug(string format, params object[] args) { _logger.Debug(format, args); }
		public static void Debug(object obj) { _logger.Debug(obj); }

		public static void Status(string value) { _logger.Status(value); }
		public static void Status(string format, params object[] args) { _logger.Status(format, args); }
		public static void Status(object obj) { _logger.Status(obj); }

		public static void SetFilter(LogLevel levels)
		{
			var targets = _logger.GetTargets();

			foreach (var target in targets)
				target.Filter = levels;
		}
	}
}
