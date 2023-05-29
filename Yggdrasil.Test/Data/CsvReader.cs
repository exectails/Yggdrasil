using System;
using System.IO;
using Xunit;
using Yggdrasil.Data.CSV;

namespace Yggdrasil.Test.Data
{
	public class CsvReaderTests
	{
		[Fact]
		public void Read()
		{
			var csvFile = @"
""test1"",123,true,123.4,0x12,1:2:3:4
""test2,3,4"",124,no,345.6,0x34,5:6:7:8
";

			var tmpPath = Path.GetTempFileName();
			File.WriteAllText(tmpPath, csvFile);

			using (var stream = new FileStream(tmpPath, FileMode.Open, FileAccess.Read))
			{
				var csvReader = new CsvReader(stream, tmpPath, ',');

				var lineCount = 0;
				foreach (var entry in csvReader.Next())
				{
					lineCount++;

					if (lineCount == 1)
					{
						Assert.Equal("test1", entry.ReadString(0));
						Assert.Equal(123, entry.ReadByte(1));
						Assert.Equal(123, entry.ReadSByte(1));
						Assert.Equal(123, entry.ReadShort(1));
						Assert.Equal(123, entry.ReadUShort(1));
						Assert.Equal(123, entry.ReadInt(1));
						Assert.Equal(123u, entry.ReadUInt(1));
						Assert.Equal(123, entry.ReadLong(1));
						Assert.Equal(123u, entry.ReadULong(1));
						Assert.True(entry.ReadBool(2));
						Assert.Equal(123.4f, entry.ReadFloat(3));
						Assert.Equal(123.4, entry.ReadDouble(3));
						Assert.Equal(0x12, entry.ReadByte(4));
						Assert.Equal(0x12, entry.ReadSByte(4));
						Assert.Equal(0x12, entry.ReadShort(4));
						Assert.Equal(0x12, entry.ReadUShort(4));
						Assert.Equal(0x12, entry.ReadInt(4));
						Assert.Equal(0x12u, entry.ReadUInt(4));
						Assert.Equal(0x12, entry.ReadLong(4));
						Assert.Equal(0x12u, entry.ReadULong(4));
						Assert.Equal(new[] { "1", "2", "3", "4" }, entry.ReadStringList(5));
						Assert.Equal(new[] { 1, 2, 3, 4 }, entry.ReadIntList(5));
					}
					else if (lineCount == 2)
					{
						Assert.Equal("test2,3,4", entry.ReadString(0));
						Assert.Equal(124, entry.ReadByte(1));
						Assert.Equal(124, entry.ReadSByte(1));
						Assert.Equal(124, entry.ReadShort(1));
						Assert.Equal(124, entry.ReadUShort(1));
						Assert.Equal(124, entry.ReadInt(1));
						Assert.Equal(124u, entry.ReadUInt(1));
						Assert.Equal(124, entry.ReadLong(1));
						Assert.Equal(124u, entry.ReadULong(1));
						Assert.False(entry.ReadBool(2));
						Assert.Equal(345.6f, entry.ReadFloat(3));
						Assert.Equal(345.6, entry.ReadDouble(3));
						Assert.Equal(0x34, entry.ReadByte(4));
						Assert.Equal(0x34, entry.ReadSByte(4));
						Assert.Equal(0x34, entry.ReadShort(4));
						Assert.Equal(0x34, entry.ReadUShort(4));
						Assert.Equal(0x34, entry.ReadInt(4));
						Assert.Equal(0x34u, entry.ReadUInt(4));
						Assert.Equal(0x34, entry.ReadLong(4));
						Assert.Equal(0x34u, entry.ReadULong(4));
						Assert.Equal(new[] { "5", "6", "7", "8" }, entry.ReadStringList(5));
						Assert.Equal(new[] { 5, 6, 7, 8 }, entry.ReadIntList(5));
					}

					Assert.Throws<IndexOutOfRangeException>(() => entry.ReadByte(10));
					Assert.Throws<IndexOutOfRangeException>(() => entry.ReadSByte(10));
					Assert.Throws<IndexOutOfRangeException>(() => entry.ReadShort(10));
					Assert.Throws<IndexOutOfRangeException>(() => entry.ReadUShort(10));
					Assert.Throws<IndexOutOfRangeException>(() => entry.ReadInt(10));
					Assert.Throws<IndexOutOfRangeException>(() => entry.ReadUInt(10));
					Assert.Throws<IndexOutOfRangeException>(() => entry.ReadLong(10));
					Assert.Throws<IndexOutOfRangeException>(() => entry.ReadULong(10));
					Assert.Throws<IndexOutOfRangeException>(() => entry.ReadFloat(10));
					Assert.Throws<IndexOutOfRangeException>(() => entry.ReadDouble(10));
					Assert.Throws<IndexOutOfRangeException>(() => entry.ReadString(10));
					Assert.Throws<IndexOutOfRangeException>(() => entry.ReadStringList(10));
					Assert.Throws<IndexOutOfRangeException>(() => entry.ReadIntList(10));
				}

				Assert.Equal(2, lineCount);
			}
		}
	}
}
