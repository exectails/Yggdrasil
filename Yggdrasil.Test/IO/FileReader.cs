// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;
using System.IO;
using Xunit;
using Yggdrasil.IO;

namespace Yggdrasil.Test.IO
{
	public class FileReaderTests
	{
		/// <summary>
		/// Tests simple lines, comments, and empty lines.
		/// </summary>
		[Fact]
		public void Reading()
		{
			var tmpFile = Path.GetTempFileName();
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

				int i = 0;
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
			var tmpFile1 = Path.GetTempFileName();
			var tmpFile2 = Path.GetTempFileName();

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
			var tmpFile1 = Path.GetTempFileName();

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
			var tmpFile1 = Path.GetTempFileName();
			var tmpFile2 = Path.GetTempFileName();

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

				Assert.Throws(typeof(FileNotFoundException), () =>
				{
					foreach (var line in fr)
						lines.Add(line.Value);

					Assert.Equal(new string[] { "line1" }, lines);
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
			var tmpFile1 = Path.GetTempFileName();
			var tmpFile2 = Path.GetTempFileName();

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
		/// Tests including a file from the same folder by its name.
		/// </summary>
		[Fact]
		public void RelativeIncludingSameFolder()
		{
			var tmpFile1 = Path.GetTempFileName();
			var tmpFile2 = Path.GetTempFileName();
			var name2 = Path.GetFileName(tmpFile2);

			File.WriteAllText(tmpFile1, @"
line1
include """ + name2 + @"""
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

				Assert.Equal(new string[] { "line1", "line2", "line3" }, lines);
			}
		}

		/// <summary>
		/// Tests including from sub-folders by their relative path.
		/// </summary>
		[Fact]
		public void RelativeIncluding()
		{
			var tmpFolder1 = Path.Combine(Path.GetTempPath(), "FileReaderTmp1");
			var tmpFolder2 = Path.Combine(Path.GetTempPath(), "FileReaderTmp1", "FileReaderTmp2");

			if (!Directory.Exists(tmpFolder2))
				Directory.CreateDirectory(tmpFolder2);

			var tmpFile1 = Path.GetTempFileName();
			var tmpFile2 = Path.Combine(tmpFolder1, "temp2");
			var tmpFile3 = Path.Combine(tmpFolder2, "temp3");

			File.WriteAllText(tmpFile1, @"
line1
include ""FileReaderTmp1/temp2""
line5
");

			File.WriteAllText(tmpFile2, @"
line2
include ""FileReaderTmp2/temp3""
line4
");

			File.WriteAllText(tmpFile3, @"
line3
");

			using (var fr = new FileReader(tmpFile1))
			{
				var lines = new List<string>();

				int i = 0;
				foreach (var line in fr)
				{
					switch (i)
					{
						case 0: Assert.Equal(tmpFile1, line.File); break;
						case 1: Assert.Equal(tmpFile2, line.File); break;
						case 2: Assert.Equal(tmpFile3, line.File); break;
						case 3: Assert.Equal(tmpFile2, line.File); break;
						case 4: Assert.Equal(tmpFile1, line.File); break;
					}

					lines.Add(line.Value);

					i++;
				}

				Assert.Equal(new string[] { "line1", "line2", "line3", "line4", "line5" }, lines);
			}
		}
	}
}
