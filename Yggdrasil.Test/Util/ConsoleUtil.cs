// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;
using Yggdrasil.Util;

namespace Yggdrasil.Test.Util
{
	public class ConsoleUtilTests
	{
		private readonly static string[] Logo = new string[]
		{
			@"_____.___.                 .___                    .__.__   ",
			@"\__  |   | ____   ____   __| _/___________    _____|__|  |  ",
			@" /   |   |/ ___\ / ___\ / __ |\_  __ \__  \  /  ___/  |  |  ",
			@" \____   / /_/  > /_/  > /_/ | |  | \// __ \_\___ \|  |  |__",
			@" / ______\___  /\___  /\____ | |__|  (____  /____  >__|____/",
			@" \/     /_____//_____/      \/            \/     \/         ",
		};

		private readonly static string[] Credits = new string[]
		{
			@"by the Aura development team",
			@"test line",
		};

		[Fact]
		public void WriteHeader()
		{
			if (Console.WindowWidth != 80)
			{
				Console.WriteLine("Skipping test WriteHeader, incompatible console width.");
				return;
			}

			var sw = new StringWriter();
			var cout = Console.Out;
			Console.SetOut(sw);

			var foreColor = Console.ForegroundColor;
			var backColor = Console.BackgroundColor;

			ConsoleUtil.WriteHeader("Yggdrasil", "Tests", Logo, ConsoleColor.Blue, Credits);

			Assert.Equal(foreColor, Console.ForegroundColor);
			Assert.Equal(backColor, Console.BackgroundColor);
			Assert.Equal("Yggdrasil : Tests", Console.Title);

			var output = sw.ToString();
			Assert.Equal(string.Join(sw.NewLine, new string[]{
@"          _____.___.                 .___                    .__.__   ",
@"          \__  |   | ____   ____   __| _/___________    _____|__|  |  ",
@"           /   |   |/ ___\ / ___\ / __ |\_  __ \__  \  /  ___/  |  |  ",
@"           \____   / /_/  > /_/  > /_/ | |  | \// __ \_\___ \|  |  |__",
@"           / ______\___  /\___  /\____ | |__|  (____  /____  >__|____/",
@"           \/     /_____//_____/      \/            \/     \/         ",
@"",
@"                          by the Aura development team",
@"                          test line",
@"________________________________________________________________________________",
@""}), output);

			Console.SetOut(cout);
		}

		[Fact]
		public void WriteSeperator()
		{
			// Currently failing on Travis for some reason o.o
			if (ConsoleUtil.CheckMono())
				return;

			var cout = Console.Out;

			var sw1 = new StringWriter();
			Console.SetOut(sw1);

			ConsoleUtil.WriteSeperator();

			var width1 = Console.WindowWidth;
			var output1 = sw1.ToString();

			Assert.Equal(width1 + sw1.NewLine.Length, output1.Length);
			Assert.True(Regex.IsMatch(output1, "^_+" + sw1.NewLine + "$"));

			var widthSettingSupported = false;
			try
			{
				Console.WindowWidth = 90;
				widthSettingSupported = true;
			}
			catch (IOException)
			{
				Console.WriteLine("Skipping part of test WriteSeperator, IOException...?");
			}
			catch (NotSupportedException)
			{
				Console.WriteLine("Skipping part of test WriteSeperator, setting of console width not supported.");
			}

			if (widthSettingSupported)
			{
				var sw2 = new StringWriter();
				Console.SetOut(sw2);

				ConsoleUtil.WriteSeperator();

				var width2 = Console.WindowWidth;
				var output2 = sw2.ToString();

				Assert.Equal(width2 + sw2.NewLine.Length, output2.Length);
				Assert.True(Regex.IsMatch(output2, "^_+" + sw2.NewLine + "$"));

				Console.WindowWidth = width1;
			}

			Console.SetOut(cout);
		}

		[Fact]
		public void LoadingTitle()
		{
			// Currently failing on Travis for some reason o.o
			if (ConsoleUtil.CheckMono())
				return;

			var sw = new StringWriter();
			var cout = Console.Out;
			Console.SetOut(sw);

			ConsoleUtil.WriteHeader("Yggdrasil", "Tests", Logo, ConsoleColor.Blue, Credits);
			Assert.Equal("Yggdrasil : Tests", Console.Title);

			ConsoleUtil.LoadingTitle();
			Assert.Equal("* Yggdrasil : Tests", Console.Title);

			ConsoleUtil.RunningTitle();
			Assert.Equal("Yggdrasil : Tests", Console.Title);

			Console.SetOut(cout);
		}
	}
}
