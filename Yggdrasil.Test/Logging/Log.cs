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
			Log.Hide = 0;
			Log.LogFile = null;

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

			Log.Status("test 5");
			test += "[Status] - test 5" + sw.NewLine;
			Assert.Equal(test, sw.ToString());

			Log.Unimplemented("test 6");
			test += "[Unimplemented] - test 6" + sw.NewLine;
			Assert.Equal(test, sw.ToString());

			Log.Hide = LogLevel.Debug;
			Log.Debug("test 7");
			Assert.Equal(test, sw.ToString());

			var ex = new Exception("test 8");
			Log.Exception(ex);
			test += "[Exception] - " + ex.ToString() + sw.NewLine;
			Assert.Equal(test, sw.ToString());

			Console.SetOut(cout);
		}

		[Fact]
		public void LogToFile()
		{
			var tmpFile = Path.GetTempFileName();

			Log.Hide = 0;
			Log.LogFile = tmpFile;

			var cout = Console.Out;
			//Console.SetOut(new StringWriter());

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

			Log.Status("test 5");
			test += dPre + @"\[Status\] - test 5" + Environment.NewLine;
			Assert.Equal(true, Regex.IsMatch(File.ReadAllText(tmpFile), test));

			Log.Unimplemented("test 6");
			test += dPre + @"\[Unimplemented\] - test 6" + Environment.NewLine;
			Assert.Equal(true, Regex.IsMatch(File.ReadAllText(tmpFile), test));

			Log.Hide |= LogLevel.Debug;
			Log.Debug("test 7");
			test += dPre + @"\[Debug\] - test 7" + Environment.NewLine;
			Assert.Equal(true, Regex.IsMatch(File.ReadAllText(tmpFile), test));

			var ex = new Exception("test 8");
			Log.Exception(ex);
			test += dPre + @"\[Exception\] - " + ex.ToString() + Environment.NewLine;
			Assert.Equal(true, Regex.IsMatch(File.ReadAllText(tmpFile), test));

			Console.SetOut(cout);
		}
	}
}
