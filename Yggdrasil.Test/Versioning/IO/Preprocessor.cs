using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using Yggdrasil.Versioning.IO;

namespace Yggdrasil.Test.Versioning.IO
{
	public class PreprocessorTests
	{
		[Fact]
		public void ProcessLines()
		{
			var preprocessor = new Preprocessor();

			var contents = @"
#define FOO 1

foo
#if FOO == 1
barfoo
#endif
#if BAR
foobar
#endif
bar
";

			var bytes = Encoding.UTF8.GetBytes(contents);

			using (var stream = new MemoryStream(bytes))
			{
				var lines = preprocessor.ProcessLines("test.txt", null, stream).ToArray();

				Assert.Equal(new string[] {
					"",
					"",
					"foo",
					"barfoo",
					"bar",
				}, lines);
			}

			using (var stream = new MemoryStream(bytes))
			{
				preprocessor.Define("BAR", 1);
				var lines = preprocessor.ProcessLines("test.txt", null, stream).ToArray();

				Assert.Equal(new string[] {
					"",
					"",
					"foo",
					"barfoo",
					"foobar",
					"bar",
				}, lines);
			}
		}

		[Fact]
		public void ProcessFile()
		{
			var tempFile1 = Path.GetTempFileName();
			var tempFile2 = Path.GetTempFileName();

			try
			{
				var preprocessor = new Preprocessor();

				File.WriteAllText(tempFile1, @$"
foo
#include ""{tempFile2}""
bar
#if BARFOO == 24
foo?
#endif
".TrimStart());
				File.WriteAllText(tempFile2, @$"
barfoo
#define BARFOO 42
".TrimStart());

				var processed = preprocessor.ProcessFile(tempFile1);

				Assert.Equal(@"foo
barfoo
bar
", processed);

				var varExists = preprocessor.TryGetDefined("BARFOO", out var varVal);

				Assert.True(varExists);
				Assert.Equal(42, varVal);
			}
			finally
			{
				File.Delete(tempFile1);
				File.Delete(tempFile2);
			}
		}
	}
}
