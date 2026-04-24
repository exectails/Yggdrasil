using System;
using System.IO;
using Xunit;
using Yggdrasil.Versioning.ManagedEnum;

namespace Yggdrasil.Test.Versioning.ManagedEnum
{
	public class MEnumTests
	{
		[Fact]
		public void SetValue()
		{
			var menum = new MEnum<IdentityId>();

			menum.SetValue(IdentityId.JT_NOVICE, 0);
			menum.SetValue(IdentityId.JT_SWORDMAN, 1);
			menum.SetValue(IdentityId.JT_MAGICIAN, 2);
			menum.SetValue(IdentityId.JT_ARCHER, 3);

			Assert.Equal(0, menum.GetValue(IdentityId.JT_NOVICE));
			Assert.Equal(1, menum.GetValue(IdentityId.JT_SWORDMAN));
			Assert.Equal(2, menum.GetValue(IdentityId.JT_MAGICIAN));
			Assert.Equal(3, menum.GetValue(IdentityId.JT_ARCHER));
		}

		[Fact]
		public void ChangeValue()
		{
			var menum = new MEnum<IdentityId>();
			var version = 300;

			menum.SetValue(IdentityId.JT_NOVICE, 0);
			menum.SetValue(IdentityId.JT_SWORDMAN, 1);
			menum.SetValue(IdentityId.JT_MAGICIAN, 2);
			menum.SetValue(IdentityId.JT_ARCHER, 3);

			if (version >= 300)
			{
				menum.SetValue(IdentityId.JT_NOVICE, 45);
				menum.SetValue(IdentityId.JT_SWORDMAN, 46);
				menum.SetValue(IdentityId.JT_MAGICIAN, 47);
				menum.SetValue(IdentityId.JT_ARCHER, 48);
			}

			Assert.Equal(45, menum.GetValue(IdentityId.JT_NOVICE));
			Assert.Equal(46, menum.GetValue(IdentityId.JT_SWORDMAN));
			Assert.Equal(47, menum.GetValue(IdentityId.JT_MAGICIAN));
			Assert.Equal(48, menum.GetValue(IdentityId.JT_ARCHER));
		}

		[Fact]
		public void InsertValue()
		{
			var menum = new MEnum<IdentityId>();

			menum.InsertValue(IdentityId.JT_NOVICE, 45);
			menum.InsertValue(IdentityId.JT_SWORDMAN);
			menum.InsertValue(IdentityId.JT_MAGICIAN);
			menum.InsertValue(IdentityId.JT_ARCHER);

			Assert.Equal(45, menum.GetValue(IdentityId.JT_NOVICE));
			Assert.Equal(46, menum.GetValue(IdentityId.JT_SWORDMAN));
			Assert.Equal(47, menum.GetValue(IdentityId.JT_MAGICIAN));
			Assert.Equal(48, menum.GetValue(IdentityId.JT_ARCHER));

			menum.ClearValues();

			menum.InsertValue(IdentityId.JT_NOVICE);
			menum.InsertValue(IdentityId.JT_SWORDMAN);
			menum.InsertValue(IdentityId.JT_MAGICIAN, 45);
			menum.InsertValue(IdentityId.JT_ARCHER);
			menum.InsertValue(IdentityId.JT_ACOLYTE);

			Assert.Equal(0, menum.GetValue(IdentityId.JT_NOVICE));
			Assert.Equal(1, menum.GetValue(IdentityId.JT_SWORDMAN));
			Assert.Equal(45, menum.GetValue(IdentityId.JT_MAGICIAN));
			Assert.Equal(46, menum.GetValue(IdentityId.JT_ARCHER));
			Assert.Equal(47, menum.GetValue(IdentityId.JT_ACOLYTE));
		}

		[Fact]
		public void InsertValueShift()
		{
			var menum = new MEnum<IdentityId>();

			menum.InsertValue(IdentityId.JT_NOVICE);
			menum.InsertValue(IdentityId.JT_SWORDMAN);
			menum.InsertValue(IdentityId.JT_MAGICIAN);
			menum.InsertValue(IdentityId.JT_ARCHER);
			menum.InsertValue(IdentityId.JT_ACOLYTE);
			menum.InsertValue(IdentityId.JT_MERCHANT);
			menum.InsertValue(IdentityId.JT_THIEF);

			menum.InsertValue(IdentityId.JT_ARCHER, 45);

			Assert.Equal(0, menum.GetValue(IdentityId.JT_NOVICE));
			Assert.Equal(1, menum.GetValue(IdentityId.JT_SWORDMAN));
			Assert.Equal(2, menum.GetValue(IdentityId.JT_MAGICIAN));
			Assert.Equal(45, menum.GetValue(IdentityId.JT_ARCHER));
			Assert.Equal(46, menum.GetValue(IdentityId.JT_ACOLYTE));
			Assert.Equal(47, menum.GetValue(IdentityId.JT_MERCHANT));
			Assert.Equal(48, menum.GetValue(IdentityId.JT_THIEF));
		}

		[Fact]
		public void InsertValueShiftGap()
		{
			var menum = new MEnum<IdentityId>();

			menum.InsertValue(IdentityId.JT_NOVICE, 0);
			menum.InsertValue(IdentityId.JT_SWORDMAN, 1);
			menum.InsertValue(IdentityId.JT_MAGICIAN, 2);
			menum.InsertValue(IdentityId.JT_ARCHER, 100);
			menum.InsertValue(IdentityId.JT_ACOLYTE, 101);
			menum.InsertValue(IdentityId.JT_MERCHANT, 102);
			menum.InsertValue(IdentityId.JT_THIEF, 103);

			menum.InsertValue(IdentityId.JT_ARCHER, 45);

			Assert.Equal(0, menum.GetValue(IdentityId.JT_NOVICE));
			Assert.Equal(1, menum.GetValue(IdentityId.JT_SWORDMAN));
			Assert.Equal(2, menum.GetValue(IdentityId.JT_MAGICIAN));
			Assert.Equal(45, menum.GetValue(IdentityId.JT_ARCHER));
			Assert.Equal(101, menum.GetValue(IdentityId.JT_ACOLYTE));
			Assert.Equal(102, menum.GetValue(IdentityId.JT_MERCHANT));
			Assert.Equal(103, menum.GetValue(IdentityId.JT_THIEF));

			menum.InsertValue(IdentityId.JT_ARCHER, 200);

			Assert.Equal(0, menum.GetValue(IdentityId.JT_NOVICE));
			Assert.Equal(1, menum.GetValue(IdentityId.JT_SWORDMAN));
			Assert.Equal(2, menum.GetValue(IdentityId.JT_MAGICIAN));
			Assert.Equal(200, menum.GetValue(IdentityId.JT_ARCHER));
			Assert.Equal(201, menum.GetValue(IdentityId.JT_ACOLYTE));
			Assert.Equal(202, menum.GetValue(IdentityId.JT_MERCHANT));
			Assert.Equal(203, menum.GetValue(IdentityId.JT_THIEF));
		}

		[Fact]
		public void InsertValueShiftRepeat()
		{
			var menum = new MEnum<IdentityId>();

			menum.InsertValue(IdentityId.JT_NOVICE);
			menum.InsertValue(IdentityId.JT_SWORDMAN);
			menum.InsertValue(IdentityId.JT_MAGICIAN);
			menum.InsertValue(IdentityId.JT_ARCHER);
			menum.InsertValue(IdentityId.JT_ACOLYTE);
			menum.InsertValue(IdentityId.JT_MERCHANT);
			menum.InsertValue(IdentityId.JT_THIEF);

			Assert.Equal(0, menum.GetValue(IdentityId.JT_NOVICE));
			Assert.Equal(1, menum.GetValue(IdentityId.JT_SWORDMAN));
			Assert.Equal(2, menum.GetValue(IdentityId.JT_MAGICIAN));
			Assert.Equal(3, menum.GetValue(IdentityId.JT_ARCHER));
			Assert.Equal(4, menum.GetValue(IdentityId.JT_ACOLYTE));
			Assert.Equal(5, menum.GetValue(IdentityId.JT_MERCHANT));
			Assert.Equal(6, menum.GetValue(IdentityId.JT_THIEF));

			menum.InsertValue(IdentityId.JT_NOVICE, 200);

			Assert.Equal(200, menum.GetValue(IdentityId.JT_NOVICE));
			Assert.Equal(201, menum.GetValue(IdentityId.JT_SWORDMAN));
			Assert.Equal(202, menum.GetValue(IdentityId.JT_MAGICIAN));
			Assert.Equal(203, menum.GetValue(IdentityId.JT_ARCHER));
			Assert.Equal(204, menum.GetValue(IdentityId.JT_ACOLYTE));
			Assert.Equal(205, menum.GetValue(IdentityId.JT_MERCHANT));
			Assert.Equal(206, menum.GetValue(IdentityId.JT_THIEF));
		}

		[Fact]
		public void InsertLate()
		{
			var menum = new MEnum<IdentityId>();

			menum.InsertValue(IdentityId.JT_NOVICE);
			menum.InsertValue(IdentityId.JT_SWORDMAN);
			menum.InsertValue(IdentityId.JT_MAGICIAN);
			//menum.InsertValue(IdentityId.JT_ARCHER);
			menum.InsertValue(IdentityId.JT_ACOLYTE);
			menum.InsertValue(IdentityId.JT_MERCHANT);
			menum.InsertValue(IdentityId.JT_THIEF);

			Assert.Equal(0, menum.GetValue(IdentityId.JT_NOVICE));
			Assert.Equal(1, menum.GetValue(IdentityId.JT_SWORDMAN));
			Assert.Equal(2, menum.GetValue(IdentityId.JT_MAGICIAN));
			Assert.Equal(3, menum.GetValue(IdentityId.JT_ACOLYTE));
			Assert.Equal(4, menum.GetValue(IdentityId.JT_MERCHANT));
			Assert.Equal(5, menum.GetValue(IdentityId.JT_THIEF));

			menum.InsertValue(IdentityId.JT_ARCHER, 3);

			Assert.Equal(0, menum.GetValue(IdentityId.JT_NOVICE));
			Assert.Equal(1, menum.GetValue(IdentityId.JT_SWORDMAN));
			Assert.Equal(2, menum.GetValue(IdentityId.JT_MAGICIAN));
			Assert.Equal(3, menum.GetValue(IdentityId.JT_ARCHER));
			Assert.Equal(4, menum.GetValue(IdentityId.JT_ACOLYTE));
			Assert.Equal(5, menum.GetValue(IdentityId.JT_MERCHANT));
			Assert.Equal(6, menum.GetValue(IdentityId.JT_THIEF));
		}

		[Fact]
		public void InsertFillIn()
		{
			var menum = new MEnum<IdentityId>();

			menum.InsertValue(IdentityId.JT_NOVICE);
			menum.InsertValue(IdentityId.JT_SWORDMAN);
			menum.InsertValue(IdentityId.JT_MAGICIAN);
			//menum.InsertValue(IdentityId.JT_ARCHER);
			menum.InsertValue(IdentityId.JT_ACOLYTE, 4);
			menum.InsertValue(IdentityId.JT_MERCHANT);
			menum.InsertValue(IdentityId.JT_THIEF);

			Assert.Equal(0, menum.GetValue(IdentityId.JT_NOVICE));
			Assert.Equal(1, menum.GetValue(IdentityId.JT_SWORDMAN));
			Assert.Equal(2, menum.GetValue(IdentityId.JT_MAGICIAN));
			Assert.Equal(4, menum.GetValue(IdentityId.JT_ACOLYTE));
			Assert.Equal(5, menum.GetValue(IdentityId.JT_MERCHANT));
			Assert.Equal(6, menum.GetValue(IdentityId.JT_THIEF));

			menum.InsertValue(IdentityId.JT_ARCHER, 3);

			Assert.Equal(0, menum.GetValue(IdentityId.JT_NOVICE));
			Assert.Equal(1, menum.GetValue(IdentityId.JT_SWORDMAN));
			Assert.Equal(2, menum.GetValue(IdentityId.JT_MAGICIAN));
			Assert.Equal(3, menum.GetValue(IdentityId.JT_ARCHER));
			Assert.Equal(4, menum.GetValue(IdentityId.JT_ACOLYTE));
			Assert.Equal(5, menum.GetValue(IdentityId.JT_MERCHANT));
			Assert.Equal(6, menum.GetValue(IdentityId.JT_THIEF));
		}

		[Fact]
		public void HasValue()
		{
			var menum = new MEnum<IdentityId>();

			menum.InsertValue(IdentityId.JT_NOVICE);
			menum.InsertValue(IdentityId.JT_SWORDMAN);
			menum.InsertValue(IdentityId.JT_MAGICIAN);
			menum.InsertValue(IdentityId.JT_ARCHER);
			menum.InsertValue(IdentityId.JT_ACOLYTE);
			menum.InsertValue(IdentityId.JT_MERCHANT);
			menum.InsertValue(IdentityId.JT_THIEF);

			Assert.True(menum.HasValue(IdentityId.JT_NOVICE));
			Assert.True(menum.HasValue(IdentityId.JT_SWORDMAN));
			Assert.True(menum.HasValue(IdentityId.JT_MAGICIAN));
			Assert.True(menum.HasValue(IdentityId.JT_ARCHER));
			Assert.True(menum.HasValue(IdentityId.JT_ACOLYTE));
			Assert.True(menum.HasValue(IdentityId.JT_MERCHANT));
			Assert.True(menum.HasValue(IdentityId.JT_THIEF));
			Assert.False(menum.HasValue(IdentityId.JT_KNIGHT));
		}

		[Fact]
		public void GetDefault()
		{
			var menum = new MEnum<IdentityId>();

			menum.InsertValue(IdentityId.JT_NOVICE);
			menum.InsertValue(IdentityId.JT_SWORDMAN);
			menum.InsertValue(IdentityId.JT_MAGICIAN);
			menum.InsertValue(IdentityId.JT_ARCHER);
			menum.InsertValue(IdentityId.JT_ACOLYTE);
			menum.InsertValue(IdentityId.JT_MERCHANT);
			menum.InsertValue(IdentityId.JT_THIEF);

			Assert.Throws<ArgumentException>(() => menum.GetValue(IdentityId.JT_KNIGHT));
			Assert.Equal(-1, menum.GetValue(IdentityId.JT_KNIGHT, -1));
			Assert.False(menum.TryGetValue(IdentityId.JT_KNIGHT, out _));
		}

		[Fact]
		public void VersionDifferences()
		{
			var menum = new MEnum<IdentityId>();

			LoadVersion(menum, 0);

			Assert.Equal(0, menum.GetValue(IdentityId.JT_NOVICE));
			Assert.Equal(1, menum.GetValue(IdentityId.JT_SWORDMAN));
			Assert.Equal(2, menum.GetValue(IdentityId.JT_MAGICIAN));
			Assert.Equal(3, menum.GetValue(IdentityId.JT_ARCHER));
			Assert.Equal(4, menum.GetValue(IdentityId.JT_ACOLYTE));
			Assert.Equal(5, menum.GetValue(IdentityId.JT_MERCHANT));
			Assert.Equal(6, menum.GetValue(IdentityId.JT_THIEF));
			Assert.Throws<ArgumentException>(() => menum.GetValue(IdentityId.JT_KNIGHT));

			LoadVersion(menum, 100);

			Assert.Equal(1001, menum.GetValue(IdentityId.JT_NOVICE));
			Assert.Equal(1002, menum.GetValue(IdentityId.JT_SWORDMAN));
			Assert.Equal(1003, menum.GetValue(IdentityId.JT_MAGICIAN));
			Assert.Equal(1004, menum.GetValue(IdentityId.JT_ARCHER));
			Assert.Equal(1005, menum.GetValue(IdentityId.JT_ACOLYTE));
			Assert.Equal(1006, menum.GetValue(IdentityId.JT_MERCHANT));
			Assert.Equal(1007, menum.GetValue(IdentityId.JT_THIEF));
			Assert.Throws<ArgumentException>(() => menum.GetValue(IdentityId.JT_KNIGHT));

			LoadVersion(menum, 200);

			Assert.Equal(1001, menum.GetValue(IdentityId.JT_NOVICE));
			Assert.Equal(1002, menum.GetValue(IdentityId.JT_SWORDMAN));
			Assert.Equal(1003, menum.GetValue(IdentityId.JT_MAGICIAN));
			Assert.Equal(1004, menum.GetValue(IdentityId.JT_ARCHER));
			Assert.Equal(1005, menum.GetValue(IdentityId.JT_ACOLYTE));
			Assert.Equal(1006, menum.GetValue(IdentityId.JT_MERCHANT));
			Assert.Equal(1007, menum.GetValue(IdentityId.JT_THIEF));
			Assert.Equal(2000, menum.GetValue(IdentityId.JT_KNIGHT));

			LoadVersion(menum, 300);

			Assert.Equal(10, menum.GetValue(IdentityId.JT_KNIGHT));
			Assert.Equal(1001, menum.GetValue(IdentityId.JT_NOVICE));
			Assert.Equal(1002, menum.GetValue(IdentityId.JT_SWORDMAN));
			Assert.Equal(1003, menum.GetValue(IdentityId.JT_MAGICIAN));
			Assert.Equal(2001, menum.GetValue(IdentityId.JT_ARCHER));
			Assert.Equal(2002, menum.GetValue(IdentityId.JT_ACOLYTE));
			Assert.Equal(2003, menum.GetValue(IdentityId.JT_MERCHANT));
			Assert.Equal(2004, menum.GetValue(IdentityId.JT_THIEF));
		}

		private static void LoadVersion(MEnum<IdentityId> menum, int version)
		{
			menum.ClearValues();

			if (version < 100)
				menum.InsertValue(IdentityId.JT_NOVICE, 0);
			else
				menum.InsertValue(IdentityId.JT_NOVICE, 1001);

			menum.InsertValue(IdentityId.JT_SWORDMAN);
			menum.InsertValue(IdentityId.JT_MAGICIAN);
			menum.InsertValue(IdentityId.JT_ARCHER);
			menum.InsertValue(IdentityId.JT_ACOLYTE);
			menum.InsertValue(IdentityId.JT_MERCHANT);
			menum.InsertValue(IdentityId.JT_THIEF);

			if (version >= 200)
				menum.InsertValue(IdentityId.JT_KNIGHT, 2000);

			if (version >= 300)
			{
				menum.InsertValue(IdentityId.JT_KNIGHT, 10);
				menum.InsertValue(IdentityId.JT_ARCHER, 2001);
			}
		}

		[Fact]
		public void GetKey()
		{
			var menum = new MEnum<IdentityId>();

			LoadVersion(menum, 300);

			Assert.Equal(10, menum.GetValue(IdentityId.JT_KNIGHT));
			Assert.Equal(1001, menum.GetValue(IdentityId.JT_NOVICE));
			Assert.Equal(1002, menum.GetValue(IdentityId.JT_SWORDMAN));
			Assert.Equal(1003, menum.GetValue(IdentityId.JT_MAGICIAN));
			Assert.Equal(2001, menum.GetValue(IdentityId.JT_ARCHER));
			Assert.Equal(2002, menum.GetValue(IdentityId.JT_ACOLYTE));
			Assert.Equal(2003, menum.GetValue(IdentityId.JT_MERCHANT));
			Assert.Equal(2004, menum.GetValue(IdentityId.JT_THIEF));

			Assert.Equal(IdentityId.JT_KNIGHT, menum.GetKey(10));
			Assert.Equal(IdentityId.JT_NOVICE, menum.GetKey(1001));
			Assert.Equal(IdentityId.JT_SWORDMAN, menum.GetKey(1002));
			Assert.Equal(IdentityId.JT_MAGICIAN, menum.GetKey(1003));
			Assert.Equal(IdentityId.JT_ARCHER, menum.GetKey(2001));
			Assert.Equal(IdentityId.JT_ACOLYTE, menum.GetKey(2002));
			Assert.Equal(IdentityId.JT_MERCHANT, menum.GetKey(2003));
			Assert.Equal(IdentityId.JT_THIEF, menum.GetKey(2004));
		}

		[Fact]
		public void GetInvalidatedKey()
		{
			var menum = new MEnum<IdentityId>();

			menum.InsertValue(IdentityId.JT_NOVICE);
			menum.InsertValue(IdentityId.JT_SWORDMAN);
			menum.InsertValue(IdentityId.JT_MAGICIAN);
			menum.InsertValue(IdentityId.JT_ARCHER);
			menum.InsertValue(IdentityId.JT_ACOLYTE);
			menum.InsertValue(IdentityId.JT_MERCHANT);
			menum.InsertValue(IdentityId.JT_THIEF);

			Assert.Equal(0, menum.GetValue(IdentityId.JT_NOVICE));
			Assert.Equal(1, menum.GetValue(IdentityId.JT_SWORDMAN));
			Assert.Equal(2, menum.GetValue(IdentityId.JT_MAGICIAN));
			Assert.Equal(3, menum.GetValue(IdentityId.JT_ARCHER));
			Assert.Equal(4, menum.GetValue(IdentityId.JT_ACOLYTE));
			Assert.Equal(5, menum.GetValue(IdentityId.JT_MERCHANT));
			Assert.Equal(6, menum.GetValue(IdentityId.JT_THIEF));

			Assert.Throws<ArgumentException>(() => menum.GetKey(45));
			menum.GetKey(3); // DoesNotThrow

			Assert.True(menum.TryGetKey(3, out _));
			Assert.False(menum.TryGetKey(45, out _));

			menum.InsertValue(IdentityId.JT_ARCHER, 45);

			Assert.Throws<ArgumentException>(() => menum.GetKey(3));
			menum.GetKey(45); // DoesNotThrow

			Assert.False(menum.TryGetKey(3, out _));
			Assert.True(menum.TryGetKey(45, out _));

			Assert.Equal(45, menum.GetValue(IdentityId.JT_ARCHER));
			Assert.Equal(IdentityId.JT_ARCHER, menum.GetKey(45));

			Assert.Equal(46, menum.GetValue(IdentityId.JT_ACOLYTE));
			Assert.Equal(IdentityId.JT_ACOLYTE, menum.GetKey(46));
		}

		[Fact]
		public void GetAllValues()
		{
			var menum = new MEnum<IdentityId>();

			LoadVersion(menum, 300);

			var values = menum.GetAllValues();

			Assert.Equal(8, values.Length);

			Assert.Equal(IdentityId.JT_KNIGHT, values[0].EnumKey);
			Assert.Equal(10, values[0].Value);

			Assert.Equal(IdentityId.JT_NOVICE, values[1].EnumKey);
			Assert.Equal(1001, values[1].Value);

			Assert.Equal(IdentityId.JT_SWORDMAN, values[2].EnumKey);
			Assert.Equal(1002, values[2].Value);

			Assert.Equal(IdentityId.JT_MAGICIAN, values[3].EnumKey);
			Assert.Equal(1003, values[3].Value);

			Assert.Equal(IdentityId.JT_ARCHER, values[4].EnumKey);
			Assert.Equal(2001, values[4].Value);

			Assert.Equal(IdentityId.JT_ACOLYTE, values[5].EnumKey);
			Assert.Equal(2002, values[5].Value);

			Assert.Equal(IdentityId.JT_MERCHANT, values[6].EnumKey);
			Assert.Equal(2003, values[6].Value);

			Assert.Equal(IdentityId.JT_THIEF, values[7].EnumKey);
			Assert.Equal(2004, values[7].Value);
		}

		[Fact]
		public void LoadFile()
		{
			var menum = new MEnum<IdentityId>();
			var tempFile = Path.GetTempFileName();

			try
			{
				File.WriteAllText(tempFile, @"
// Jobs, Jobs, Jobs!
JT_NOVICE
JT_SWORDMAN
JT_MAGICIAN
JT_ARCHER
JT_ACOLYTE
JT_MERCHANT
JT_THIEF
");

				menum.LoadFile(tempFile);
			}
			finally
			{
				File.Delete(tempFile);
			}

			Assert.Equal(0, menum.GetValue(IdentityId.JT_NOVICE));
			Assert.Equal(1, menum.GetValue(IdentityId.JT_SWORDMAN));
			Assert.Equal(2, menum.GetValue(IdentityId.JT_MAGICIAN));
			Assert.Equal(3, menum.GetValue(IdentityId.JT_ARCHER));
			Assert.Equal(4, menum.GetValue(IdentityId.JT_ACOLYTE));
			Assert.Equal(5, menum.GetValue(IdentityId.JT_MERCHANT));
			Assert.Equal(6, menum.GetValue(IdentityId.JT_THIEF));
		}

		[Fact]
		public void LoadVersionedFile()
		{
			var menum = new MEnum<IdentityId>();
			var versionFile = Path.GetTempFileName();
			var tempFile = Path.GetTempFileName();

			try
			{
				File.WriteAllText(versionFile, @"
#define VERSION 300
");

				File.WriteAllText(tempFile, @$"
#include ""{versionFile}""

// Jobs, Jobs, Jobs!
JT_NOVICE = 10
JT_SWORDMAN
JT_MAGICIAN // 12
#if VERSION >= 500
	JT_ARCHER
	JT_ACOLYTE
#endif
JT_MERCHANT
JT_THIEF
");

				menum.LoadFile(tempFile);
			}
			finally
			{
				File.Delete(tempFile);
				File.Delete(versionFile);
			}

			Assert.Equal(10, menum.GetValue(IdentityId.JT_NOVICE));
			Assert.Equal(11, menum.GetValue(IdentityId.JT_SWORDMAN));
			Assert.Equal(12, menum.GetValue(IdentityId.JT_MAGICIAN));
			Assert.Equal(-1, menum.GetValue(IdentityId.JT_ARCHER, -1));
			Assert.Equal(-1, menum.GetValue(IdentityId.JT_ACOLYTE, -1));
			Assert.Equal(13, menum.GetValue(IdentityId.JT_MERCHANT));
			Assert.Equal(14, menum.GetValue(IdentityId.JT_THIEF));
		}

		public enum IdentityId
		{
			JT_NOVICE,
			JT_SWORDMAN,
			JT_MAGICIAN,
			JT_ARCHER,
			JT_ACOLYTE,
			JT_MERCHANT,
			JT_THIEF,
			JT_KNIGHT,
		}
	}
}
