// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.IO;
using Xunit;
using Yggdrasil.Configuration;

namespace Yggdrasil.Test.Configuration
{
	public class ConfFileTests
	{
		private string _testFilePath;

		private string GetTestFile()
		{
			if (_testFilePath != null)
				return _testFilePath;

			var path1 = Path.GetTempFileName();
			var path2 = Path.GetTempFileName();

			var contents1 = string.Format(@"
				test1: 10
				test2: 20
				test3: 40
				test4: 80

				// Test 5
				test5: yes
				test6: no

				test7: 2010-12-28 18:58:59
				test8: 1.02:03:04.005

				test9: Bar

				require {0}
				include {1}
			", "\"" + Path.GetFileName(path2) + "\"", "\"no_this_does_not_exist.nope\"");

			var contents2 = @"
				test10: Ygg-dra-sil!
			";

			File.WriteAllText(path1, contents1);
			File.WriteAllText(path2, contents2);

			return (_testFilePath = path1);
		}

		[Fact]
		public void Include()
		{
			var path = GetTestFile();
			var conf = new ConfFile();

			Assert.DoesNotThrow(() => conf.Include(path));
			Assert.DoesNotThrow(() => conf.Include("some_imaginary_file_that_doesnt exist.nope"));
		}

		[Fact]
		public void Require()
		{
			var path = GetTestFile();
			var conf = new ConfFile();

			Assert.DoesNotThrow(() => conf.Require(path));
			Assert.Throws<FileNotFoundException>(() => conf.Require("some_imaginary_file_that_doesnt exist.nope"));
		}

		[Fact]
		public void GetByInclude()
		{
			var path = GetTestFile();
			var conf = new ConfFile();
			conf.Include(path);

			Get(conf);
		}

		[Fact]
		public void GetByRequire()
		{
			var path = GetTestFile();
			var conf = new ConfFile();
			conf.Require(path);

			Get(conf);
		}

		public void Get(ConfFile conf)
		{
			Assert.Equal(10, conf.GetByte("test1"));
			Assert.Equal(20, conf.GetByte("test1.1", 20));
			Assert.Equal(20, conf.GetShort("test2"));
			Assert.Equal(40, conf.GetShort("test2.1", 40));
			Assert.Equal(40, conf.GetInt("test3"));
			Assert.Equal(80, conf.GetInt("test3.1", 80));
			Assert.Equal(80, conf.GetLong("test4"));
			Assert.Equal(160, conf.GetLong("test4.1", 160));

			Assert.Equal(true, conf.GetBool("test5"));
			Assert.Equal(false, conf.GetBool("test6"));
			Assert.Equal(false, conf.GetBool("test6.1"));
			Assert.Equal(true, conf.GetBool("test6.2", true));

			Assert.Equal(new DateTime(2010, 12, 28, 18, 58, 59), conf.GetDateTime("test7"));
			Assert.Equal(DateTime.Now.Date, conf.GetDateTime("test7.1", DateTime.Now.Date));

			Assert.Equal(new TimeSpan(1, 2, 3, 4, 5), conf.GetTimeSpan("test8"));
			Assert.Equal(TimeSpan.FromMinutes(5), conf.GetTimeSpan("test8.1", TimeSpan.FromMinutes(5)));

			Assert.Equal(TestEnum.Bar, conf.GetEnum<TestEnum>("test9"));
			Assert.Equal(TestEnum.Foo, conf.GetEnum<TestEnum>("test9.1", TestEnum.Foo));

			Assert.Equal("Ygg-dra-sil!", conf.GetString("test10"));
			Assert.Equal("foobar", conf.GetString("test10.1", "foobar"));
		}

		public enum TestEnum
		{
			Foo = 123,
			Bar = 456,
		}
	}
}
