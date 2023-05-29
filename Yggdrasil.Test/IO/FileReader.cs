using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using Yggdrasil.IO;

namespace Yggdrasil.Test.IO
{
	public class FileReaderTests
	{
		private static int _tmp;

		/// <summary>
		/// Creates a temp file to be used by a test and returns
		/// its relative path.
		/// </summary>
		/// <returns></returns>
		private static string GetTempFileName()
		{
			var result = "tmp" + _tmp++;

			File.WriteAllText(result, "");

			return result;
		}

		/// <summary>
		/// Tests simple lines, comments, and empty lines.
		/// </summary>
		[Fact]
		public void Reading()
		{
			var tmpFile = GetTempFileName();
			File.WriteAllText(tmpFile, @"
line1
line2
# comment1
-- comment2
// comment3
; comment4
! comment5

line3
");

			using (var fr = new FileReader(tmpFile))
			{
				var lines = new List<string>();

				var i = 0;
				foreach (var line in fr)
				{
					switch (i)
					{
						case 0: Assert.Equal(2, fr.CurrentLine); break;
						case 1: Assert.Equal(3, fr.CurrentLine); break;
						case 2: Assert.Equal(10, fr.CurrentLine); break;
					}

					lines.Add(line.Value);

					i++;
				}

				Assert.Equal(new string[] { "line1", "line2", "line3" }, lines);
			}
		}

		/// <summary>
		/// Tests including a file with full path.
		/// </summary>
		[Fact]
		public void Including()
		{
			var tmpFile1 = GetTempFileName();
			var tmpFile2 = GetTempFileName();

			File.WriteAllText(tmpFile1, @"
line1
include """ + tmpFile2 + @"""
line5
");

			File.WriteAllText(tmpFile2, @"
line2
line4
line3
");

			using (var fr = new FileReader(tmpFile1))
			{
				var lines = new List<string>();

				foreach (var line in fr)
					lines.Add(line.Value);

				Assert.Equal(new string[] { "line1", "line2", "line4", "line3", "line5" }, lines);
			}
		}

		/// <summary>
		/// Tests including a file that doesn't exist. Non-existing files are
		/// ignored.
		/// </summary>
		[Fact]
		public void IncludingNonExistent()
		{
			var tmpFile1 = GetTempFileName();

			File.WriteAllText(tmpFile1, @"
line1
include ""_shouldntexistunlesswereveryveryverylucky_""
");

			using (var fr = new FileReader(tmpFile1))
			{
				var lines = new List<string>();

				foreach (var line in fr)
					lines.Add(line.Value);

				Assert.Equal(new string[] { "line1" }, lines);
			}
		}

		/// <summary>
		/// Tests requiring a file that does and one that does not exist.
		/// Unlike including, requiring a non-existing file throws an
		/// exception.
		/// </summary>
		[Fact]
		public void Require()
		{
			var tmpFile1 = GetTempFileName();
			var tmpFile2 = GetTempFileName();

			File.WriteAllText(tmpFile1, @"
line1
require ""_shouldntexistunlesswereveryveryverylucky_""
line5
");

			File.WriteAllText(tmpFile2, @"
line2
line4
line3
");

			using (var fr = new FileReader(tmpFile1))
			{
				var lines = new List<string>();

				Assert.Throws<FileNotFoundException>(() =>
				{
					foreach (var line in fr)
						lines.Add(line.Value);

					//Assert.Equal(new string[] { "line1" }, lines);
				});
			}

			File.WriteAllText(tmpFile1, @"
line1
require """ + tmpFile2 + @"""
line5
");

			using (var fr = new FileReader(tmpFile1))
			{
				var lines = new List<string>();

				foreach (var line in fr)
					lines.Add(line.Value);

				Assert.Equal(new string[] { "line1", "line2", "line4", "line3", "line5" }, lines);
			}
		}

		/// <summary>
		/// Tests diverting, meaning continuing to read from an included
		/// file without returning to the original one.
		/// </summary>
		[Fact]
		public void Divert()
		{
			var tmpFile1 = GetTempFileName();
			var tmpFile2 = GetTempFileName();

			File.WriteAllText(tmpFile1, @"
line1
divert """ + tmpFile2 + @"""
line3
");

			File.WriteAllText(tmpFile2, @"
line2
");

			using (var fr = new FileReader(tmpFile1))
			{
				var lines = new List<string>();

				foreach (var line in fr)
					lines.Add(line.Value);

				Assert.Equal(new string[] { "line1", "line2" }, lines);
			}
		}

		/// <summary>
		/// Tests including from sub-folders by their relative path.
		/// </summary>
		[Fact]
		public void IncludingFromSubfolders()
		{
			var tmpFolder1 = "FileReaderTmp1";
			var tmpFolder2 = "FileReaderTmp2";
			var tmpFolder1_2 = Path.Combine(tmpFolder1, tmpFolder2);

			if (!Directory.Exists(tmpFolder1_2))
				Directory.CreateDirectory(tmpFolder1_2);

			var tmpFile1 = GetTempFileName();
			var tmpFile2 = Path.Combine(tmpFolder1, "temp2");
			var tmpFile3 = Path.Combine(tmpFolder2, "temp3");
			var tmpFile4 = GetTempFileName();
			var tmpFile1Full = Path.GetFullPath(tmpFile1);
			var tmpFile2Full = Path.GetFullPath(tmpFile2);
			var tmpFile3Full = Path.GetFullPath(Path.Combine(tmpFolder1_2, "temp3"));
			var tmpFile4Full = Path.GetFullPath(tmpFile4);

			File.WriteAllText(tmpFile1Full, @"
line1
include """ + tmpFile2 + @"""
line5
");

			File.WriteAllText(tmpFile2Full, @"
line2
require """ + tmpFile3 + @"""
line4
");

			File.WriteAllText(tmpFile3Full, @"
include ""/" + tmpFile4 + @"""
line3
");

			File.WriteAllText(tmpFile4Full, @"
lineXY
");

			using (var fr = new FileReader(tmpFile1))
			{
				var lines = new List<string>();

				var i = 0;
				foreach (var line in fr)
				{
					switch (i)
					{
						case 0: Assert.Equal(tmpFile1Full, line.File); break;
						case 1: Assert.Equal(tmpFile2Full, line.File); break;
						case 2: Assert.Equal(tmpFile4Full, line.File); break;
						case 3: Assert.Equal(tmpFile3Full, line.File); break;
						case 4: Assert.Equal(tmpFile2Full, line.File); break;
						case 5: Assert.Equal(tmpFile1Full, line.File); break;
					}

					lines.Add(line.Value);

					i++;
				}

				Assert.Equal(new string[] { "line1", "line2", "lineXY", "line3", "line4", "line5" }, lines);
			}
		}
	}
}
