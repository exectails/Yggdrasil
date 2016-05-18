// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.IO;
using Xunit;
using Yggdrasil.Util;

namespace Yggdrasil.Test.Util
{
	public class ConfFileTests
	{
		[Fact]
		public void Reading()
		{
			var tmpFile = Path.GetTempFileName();
			File.WriteAllText(tmpFile, @"
test1: yes

// Test 2
test2: 100

test3:  30000
test4:  70000
test5:  3000000000
test6:  foobar
test7:  42.5
test8:  23:59
test9:  5
test10: 10.02:03:04.005
test11: Bar
");

			var conf = new ConfFile();
			var now = DateTime.Now;

			conf.Include("_shouldntexistunlesswereveryveryverylucky_");

			Assert.Equal(-1, conf.GetInt("test1", -1));

			conf.Include(tmpFile);

			Assert.Equal(true, conf.GetBool("test1", false));
			Assert.Equal(false, conf.GetBool("test42", false));
			Assert.Equal(true, conf.GetBool("test43", true));

			Assert.Equal(100, conf.GetByte("test2", 0));
			Assert.Equal(5, conf.GetByte("test44", 5));

			Assert.Equal(30000, conf.GetShort("test3", 0));
			Assert.Equal(6, conf.GetShort("test45", 6));

			Assert.Equal(70000, conf.GetInt("test4", 0));
			Assert.Equal(7, conf.GetInt("test46", 7));

			Assert.Equal(3000000000, conf.GetLong("test5", 0));
			Assert.Equal(8, conf.GetInt("test47", 8));

			Assert.Equal("foobar", conf.GetString("test6", "0"));
			Assert.Equal("9", conf.GetString("test48", "9"));

			Assert.Equal(42.5f, conf.GetFloat("test7", 0));
			Assert.Equal(0.123f, conf.GetFloat("test49", 0.123f));

			Assert.Equal(TimeSpan.FromHours(23) + TimeSpan.FromMinutes(59), conf.GetTimeSpan("test8", TimeSpan.MinValue));
			Assert.Equal(TimeSpan.FromDays(5), conf.GetTimeSpan("test9", TimeSpan.MinValue));
			Assert.Equal(TimeSpan.FromDays(10) + TimeSpan.FromHours(2) + TimeSpan.FromMinutes(3) + TimeSpan.FromSeconds(4) + TimeSpan.FromMilliseconds(5), conf.GetTimeSpan("test10", TimeSpan.MinValue));
			Assert.Equal(TimeSpan.FromHours(24), conf.GetTimeSpan("test50", now - now.AddDays(-1)));

			Assert.Equal(Test.Bar, conf.GetEnum("test11", Test.Null));
			Assert.Equal(Test.Foo, conf.GetEnum("test51", Test.Foo));
		}

		private enum Test
		{
			Null = 0,
			Foo = 100,
			Bar = 200,
		}
	}
}
