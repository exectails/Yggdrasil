// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;
using Yggdrasil.Logging;

namespace Yggdrasil.Test.Logging
{
	public class LogTests
	{
		[Fact]
		public void LogToConsole()
		{
			var filter = LogLevel.None;

			var sw = new StringWriter();
			var cout = Console.Out;
			Console.SetOut(sw);

			var test = "[Info] - test 1" + sw.NewLine;

			Log.Info("test 1");
			Assert.Equal(test, sw.ToString());

			Log.Warning("test 2");
			test += "[Warning] - test 2" + sw.NewLine;
			Assert.Equal(test, sw.ToString());

			Log.Error("test 3");
			test += "[Error] - test 3" + sw.NewLine;
			Assert.Equal(test, sw.ToString());

			Log.Debug("test 4");
			test += "[Debug] - test 4" + sw.NewLine;
			Assert.Equal(test, sw.ToString());

			filter = LogLevel.Debug;
			Log.SetFilter(filter);

			Log.Debug("test 5");
			Assert.Equal(test, sw.ToString());

			Log.Status("test 6");
			test += "[Status] - test 6" + sw.NewLine;
			Assert.Equal(test, sw.ToString());

			Console.SetOut(cout);
		}

		[Fact]
		public void LogToFile()
		{
			var tmpFile = Path.GetTempFileName();

			var filter = LogLevel.None;
			var cout = Console.Out;

			var dPre = "[0-9]{4}-[0-9]{2}-[0-9]{2} [0-9]{2}:[0-9]{2} ";
			var test = dPre + @"\[Info\] - test 1" + Environment.NewLine;

			Log.Info("test 1");
			Assert.Equal(true, Regex.IsMatch(File.ReadAllText(tmpFile), test));

			Log.Warning("test 2");
			test += dPre + @"\[Warning\] - test 2" + Environment.NewLine;
			Assert.Equal(true, Regex.IsMatch(File.ReadAllText(tmpFile), test));

			Log.Error("test 3");
			test += dPre + @"\[Error\] - test 3" + Environment.NewLine;
			Assert.Equal(true, Regex.IsMatch(File.ReadAllText(tmpFile), test));

			Log.Debug("test 4");
			test += dPre + @"\[Debug\] - test 4" + Environment.NewLine;
			Assert.Equal(true, Regex.IsMatch(File.ReadAllText(tmpFile), test));

			filter = LogLevel.Debug;
			Log.SetFilter(filter);

			Log.Debug("test 5");
			test += dPre + @"\[Debug\] - test 5" + Environment.NewLine;
			Assert.Equal(true, Regex.IsMatch(File.ReadAllText(tmpFile), test));

			Log.Status("test 6");
			test += dPre + @"\[Status\] - test 6" + Environment.NewLine;
			Assert.Equal(true, Regex.IsMatch(File.ReadAllText(tmpFile), test));

			Console.SetOut(cout);
		}
	}
}
