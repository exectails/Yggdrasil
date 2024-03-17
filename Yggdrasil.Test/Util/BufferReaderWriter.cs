using System;
using System.IO;
using System.Text;
using Xunit;
using Yggdrasil.Util;

namespace Yggdrasil.Test.Util
{
	public class BufferReaderWriterTests
	{
		[Fact]
		public void Writing()
		{
			var buffer = new BufferReaderWriter();
			buffer.WriteByte(0x42);
			Assert.Equal(new byte[] { 0x42 }, buffer.Copy());

			buffer = new BufferReaderWriter();
			buffer.WriteInt16(0x4243);
			Assert.Equal(new byte[] { 0x42, 0x43 }, buffer.Copy());

			buffer = new BufferReaderWriter();
			buffer.WriteInt32(0x42434445);
			Assert.Equal(new byte[] { 0x42, 0x43, 0x44, 0x45 }, buffer.Copy());

			buffer = new BufferReaderWriter();
			buffer.WriteInt64(0x4243444546474849);
			Assert.Equal(new byte[] { 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49 }, buffer.Copy());

			buffer = new BufferReaderWriter();
			buffer.WriteFloat(42.43f);
			Assert.Equal(new byte[] { 0x42, 0x29, 0xB8, 0x52 }, buffer.Copy());

			buffer = new BufferReaderWriter();
			buffer.WriteDouble(42.43);
			Assert.Equal(new byte[] { 0x40, 0x45, 0x37, 0x0A, 0x3D, 0x70, 0xA3, 0xD7 }, buffer.Copy());

			buffer = new BufferReaderWriter();
			buffer.Write(new byte[] { 0x42, 0x43, 0x44, 0x45 });
			Assert.Equal(new byte[] { 0x42, 0x43, 0x44, 0x45 }, buffer.Copy());
		}

		[Fact]
		public void WritingLE()
		{
			var buffer = new BufferReaderWriter() { Endianness = Endianness.LittleEndian };
			buffer.WriteByte(0x42);
			Assert.Equal(new byte[] { 0x42 }, buffer.Copy());

			buffer = new BufferReaderWriter() { Endianness = Endianness.LittleEndian };
			buffer.WriteInt16(0x4243);
			Assert.Equal(new byte[] { 0x43, 0x42 }, buffer.Copy());

			buffer = new BufferReaderWriter() { Endianness = Endianness.LittleEndian };
			buffer.WriteInt32(0x42434445);
			Assert.Equal(new byte[] { 0x45, 0x44, 0x43, 0x42 }, buffer.Copy());

			buffer = new BufferReaderWriter() { Endianness = Endianness.LittleEndian };
			buffer.WriteInt64(0x4243444546474849);
			Assert.Equal(new byte[] { 0x49, 0x48, 0x47, 0x46, 0x45, 0x44, 0x43, 0x42 }, buffer.Copy());

			buffer = new BufferReaderWriter() { Endianness = Endianness.LittleEndian };
			buffer.WriteFloat(42.43f);
			Assert.Equal(new byte[] { 0x52, 0xB8, 0x29, 0x42 }, buffer.Copy());

			buffer = new BufferReaderWriter() { Endianness = Endianness.LittleEndian };
			buffer.WriteDouble(42.43);
			Assert.Equal(new byte[] { 0xD7, 0xA3, 0x70, 0x3D, 0x0A, 0x37, 0x45, 0x40 }, buffer.Copy());

			buffer = new BufferReaderWriter() { Endianness = Endianness.LittleEndian };
			buffer.Write(new byte[] { 0x42, 0x43, 0x44, 0x45 });
			Assert.Equal(new byte[] { 0x42, 0x43, 0x44, 0x45 }, buffer.Copy());
		}

		[Fact]
		public void Reading()
		{
			var buffer = new BufferReaderWriter(31);
			buffer.WriteByte(0x42);
			buffer.WriteInt16(0x4243);
			buffer.WriteInt32(0x42434445);
			buffer.WriteInt64(0x4243444546474849);
			buffer.WriteFloat(42.43f);
			buffer.WriteDouble(42.43);
			buffer.Write(new byte[] { 0x42, 0x43, 0x44, 0x45 });

			buffer.Seek(0, SeekOrigin.Begin);

			Assert.Equal(0x42, buffer.ReadByte());
			Assert.Equal(0x4243, buffer.ReadInt16());
			Assert.Equal(0x42434445, buffer.ReadInt32());
			Assert.Equal(0x4243444546474849, buffer.ReadInt64());
			Assert.Equal(42.43f, buffer.ReadFloat());
			Assert.Equal(42.43, buffer.ReadDouble());
			Assert.Equal(new byte[] { 0x42, 0x43, 0x44, 0x45 }, buffer.Read(4));

			Assert.Throws<InvalidOperationException>(() => { buffer.Read(1); });

			buffer = new BufferReaderWriter(Hex.ToByteArray("57 80 06 00 00 00 DB 9B D2 9A D2 F9"));

			Assert.Equal(0x5780u, buffer.ReadUInt16());
			Assert.Equal(0x06000000u, buffer.ReadUInt32());
			Assert.Equal(0xDB9Bu, buffer.ReadUInt16());
			Assert.Equal(0xD29AD2F9u, buffer.ReadUInt32());
		}

		[Fact]
		public void ReadingLE()
		{
			var buffer = new BufferReaderWriter(31);
			buffer.Endianness = Endianness.LittleEndian;

			buffer.WriteByte(0x42);
			buffer.WriteInt16(0x4243);
			buffer.WriteInt32(0x42434445);
			buffer.WriteInt64(0x4243444546474849);
			buffer.WriteFloat(42.43f);
			buffer.WriteDouble(42.43);
			buffer.Write(new byte[] { 0x42, 0x43, 0x44, 0x45 });

			buffer.Seek(0, SeekOrigin.Begin);

			Assert.Equal(0x42, buffer.ReadByte());
			Assert.Equal(0x4243, buffer.ReadInt16());
			Assert.Equal(0x42434445, buffer.ReadInt32());
			Assert.Equal(0x4243444546474849, buffer.ReadInt64());
			Assert.Equal(42.43f, buffer.ReadFloat());
			Assert.Equal(42.43, buffer.ReadDouble());
			Assert.Equal(new byte[] { 0x42, 0x43, 0x44, 0x45 }, buffer.Read(4));

			Assert.Throws<InvalidOperationException>(() => { buffer.Read(1); });

			buffer = new BufferReaderWriter(Hex.ToByteArray("57 80 06 00 00 00 DB 9B D2 9A D2 F9"));
			buffer.Endianness = Endianness.LittleEndian;

			Assert.Equal(0x8057u, buffer.ReadUInt16());
			Assert.Equal(0x00000006u, buffer.ReadUInt32());
			Assert.Equal(0x9BDBu, buffer.ReadUInt16());
			Assert.Equal(0xF9D29AD2u, buffer.ReadUInt32());
		}

		[Fact]
		public void Seeking()
		{
			var buffer = new BufferReaderWriter(10);
			Assert.Equal(0, buffer.Length);
			Assert.Equal(10, buffer.Capacity);

			buffer.Seek(0, SeekOrigin.Begin);
			Assert.Equal(0, buffer.Index);

			buffer.Seek(0, SeekOrigin.End);
			Assert.Equal(10, buffer.Index);

			buffer.Seek(-4, SeekOrigin.Current);
			Assert.Equal(6, buffer.Index);

			buffer.Seek(2, SeekOrigin.Current);
			Assert.Equal(8, buffer.Index);

			AssertEx.DoesNotThrow(() => { buffer.Seek(2, SeekOrigin.Current); });
			Assert.Equal(10, buffer.Index);

			buffer.Seek(-2, SeekOrigin.Current);
			Assert.Equal(8, buffer.Index);

			Assert.Throws<InvalidOperationException>(() => { buffer.Seek(3 /* => 11*/, SeekOrigin.Current); });
			Assert.Equal(8, buffer.Index);

			AssertEx.DoesNotThrow(() => { buffer.Seek(-7, SeekOrigin.Current); });
			Assert.Equal(1, buffer.Index);

			buffer.Seek(2, SeekOrigin.Current);
			Assert.Equal(3, buffer.Index);

			Assert.Throws<InvalidOperationException>(() => { buffer.Seek(-4 /* => -1*/, SeekOrigin.Current); });
			Assert.Equal(3, buffer.Index);

			AssertEx.DoesNotThrow(() => { buffer.Seek(-1, SeekOrigin.End); });
			Assert.Equal(9, buffer.Index);

			AssertEx.DoesNotThrow(() => { buffer.Seek(-5, SeekOrigin.End); });
			Assert.Equal(5, buffer.Index);

			AssertEx.DoesNotThrow(() => { buffer.Seek(-10, SeekOrigin.End); });
			Assert.Equal(0, buffer.Index);

			Assert.Throws<InvalidOperationException>(() => { buffer.Seek(1 /* => 11*/, SeekOrigin.End); });
			Assert.Equal(0, buffer.Index);

			Assert.Throws<InvalidOperationException>(() => { buffer.Seek(10, SeekOrigin.End); });
			Assert.Equal(0, buffer.Index);

			AssertEx.DoesNotThrow(() => { buffer.Seek(1, SeekOrigin.Begin); });
			Assert.Equal(1, buffer.Index);

			AssertEx.DoesNotThrow(() => { buffer.Seek(5, SeekOrigin.Begin); });
			Assert.Equal(5, buffer.Index);

			AssertEx.DoesNotThrow(() => { buffer.Seek(9, SeekOrigin.Begin); });
			Assert.Equal(9, buffer.Index);

			Assert.Throws<InvalidOperationException>(() => { buffer.Seek(-1, SeekOrigin.Begin); });
			Assert.Equal(9, buffer.Index);

			AssertEx.DoesNotThrow(() => { buffer.Seek(10, SeekOrigin.Begin); });
			Assert.Equal(10, buffer.Index);

			Assert.Throws<InvalidOperationException>(() => { buffer.Seek(11, SeekOrigin.Begin); });
			Assert.Equal(10, buffer.Index);

			buffer.Seek(5, SeekOrigin.Begin);
			AssertEx.DoesNotThrow(() => { buffer.Seek(1, SeekOrigin.Current); });
			Assert.Equal(6, buffer.Index);

			buffer.Seek(5, SeekOrigin.Begin);
			AssertEx.DoesNotThrow(() => { buffer.Seek(4, SeekOrigin.Current); });
			Assert.Equal(9, buffer.Index);

			buffer.Seek(5, SeekOrigin.Begin);
			AssertEx.DoesNotThrow(() => { buffer.Seek(-1, SeekOrigin.Current); });
			Assert.Equal(4, buffer.Index);

			buffer.Seek(5, SeekOrigin.Begin);
			AssertEx.DoesNotThrow(() => { buffer.Seek(-5, SeekOrigin.Current); });
			Assert.Equal(0, buffer.Index);

			buffer.Seek(5, SeekOrigin.Begin);
			Assert.Throws<InvalidOperationException>(() => { buffer.Seek(-6, SeekOrigin.Current); });
			Assert.Equal(5, buffer.Index);

			buffer.Seek(5, SeekOrigin.Begin);
			Assert.Throws<InvalidOperationException>(() => { buffer.Seek(6, SeekOrigin.Current); });
			Assert.Equal(5, buffer.Index);

			buffer.Seek(0, SeekOrigin.Begin);
			AssertEx.DoesNotThrow(() => { buffer.Seek(10, SeekOrigin.Begin); });
			Assert.Equal(10, buffer.Index);

			buffer.Seek(0, SeekOrigin.Begin);
			AssertEx.DoesNotThrow(() => { buffer.Seek(0, SeekOrigin.End); });
			Assert.Equal(10, buffer.Index);

			buffer.Seek(0, SeekOrigin.Begin);
			AssertEx.DoesNotThrow(() => { buffer.Seek(9, SeekOrigin.Begin); });
			AssertEx.DoesNotThrow(() => { buffer.Seek(1, SeekOrigin.Current); });
			Assert.Equal(10, buffer.Index);
		}

		[Fact]
		public void Rewriting()
		{
			var buffer = new BufferReaderWriter(10);

			buffer.WriteInt16(0);
			buffer.WriteInt64(0);

			var index = buffer.Index;
			buffer.Seek(0, SeekOrigin.Begin);

			buffer.WriteInt16(0x1111);
			buffer.WriteInt64(0x2222222222222222);

			Console.WriteLine("index: " + index);
			Console.WriteLine("capac: " + buffer.Capacity);
			buffer.Seek(index, SeekOrigin.Begin);
			Assert.Equal(10, buffer.Index);

			buffer.Seek(0, SeekOrigin.Begin);
			Assert.Equal(0x1111, buffer.ReadInt16());
			Assert.Equal(0x2222222222222222, buffer.ReadInt64());
		}

		[Fact]
		public void Resizing()
		{
			var buffer = new BufferReaderWriter(4);
			Assert.Equal(4, buffer.Capacity);

			buffer.WriteInt32(0);
			Assert.Equal(4, buffer.Capacity);

			buffer.WriteInt32(0);
			Assert.Equal(260, buffer.Capacity);
		}

		[Fact]
		public void LengthExtension()
		{
			var buffer = new BufferReaderWriter(100);

			buffer.WriteInt32(0x01020304);
			Assert.Equal(4, buffer.Length);

			buffer.Seek(2, SeekOrigin.Begin);
			buffer.WriteByte(1);
			Assert.Equal(4, buffer.Length);
			Assert.Equal(new byte[] { 1, 2, 1, 4 }, buffer.Copy());

			buffer.Seek(3, SeekOrigin.Begin);
			buffer.WriteInt16(0x0104);
			Assert.Equal(5, buffer.Length);
			Assert.Equal(new byte[] { 1, 2, 1, 1, 4 }, buffer.Copy());

			buffer.Seek(5, SeekOrigin.Begin);
			buffer.Write(new byte[] { 0x05, 0x06, 0x07 });
			Assert.Equal(8, buffer.Length);
			Assert.Equal(new byte[] { 1, 2, 1, 1, 4, 5, 6, 7 }, buffer.Copy());

			buffer.Seek(10, SeekOrigin.Begin);
			buffer.WriteByte(1);
			Assert.Equal(11, buffer.Length);
			Assert.Equal(new byte[] { 1, 2, 1, 1, 4, 5, 6, 7, 0, 0, 1 }, buffer.Copy());
		}

		[Fact]
		public void FixedLength()
		{
			var buffer = new BufferReaderWriter(new byte[10], 0, 0, false);

			buffer.WriteInt32(0x01020304);
			buffer.WriteInt32(0x01020304);
			AssertEx.DoesNotThrow(() => buffer.WriteInt32(0x01020304));

			buffer = new BufferReaderWriter(new byte[10], 0, 0, true);

			buffer.WriteInt32(0x01020304);
			buffer.WriteInt32(0x01020304);
			Assert.Throws<InvalidOperationException>(() => buffer.WriteInt32(0x01020304));
		}

		[Fact]
		public void CopyTo()
		{
			var buffer = new BufferReaderWriter(new byte[10], 0, 0, false);
			buffer.WriteByte(1);
			buffer.Seek(0, SeekOrigin.End);
			buffer.WriteByte(2);

			var arr = new byte[11];
			buffer.CopyTo(arr, 0);
			Assert.Equal(new byte[] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2 }, arr);

			Assert.Throws<InvalidOperationException>(() => buffer.CopyTo(arr, -1));

			arr = new byte[9];
			Assert.Throws<InvalidOperationException>(() => buffer.CopyTo(arr, 0));

			arr = new byte[13];
			buffer.CopyTo(arr, 2);
			Assert.Equal(new byte[] { 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2 }, arr);

			buffer = new BufferReaderWriter(new byte[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
			arr = new byte[8];
			buffer.CopyTo(arr, 0, 2);
			Assert.Equal(new byte[] { 3, 4, 5, 6, 7, 8, 9, 10 }, arr);
		}

		[Fact]
		public void ResetLength()
		{
			var buffer = new BufferReaderWriter(new byte[10], 0, 0, false);
			Assert.Equal(10, buffer.Capacity);
			Assert.Equal(0, buffer.Length);
			Assert.Equal(0, buffer.Index);

			buffer.WriteInt32(1);
			buffer.WriteInt32(2);
			Assert.Equal(10, buffer.Capacity);
			Assert.Equal(8, buffer.Length);
			Assert.Equal(8, buffer.Index);

			buffer.ResetLength();
			Assert.Equal(10, buffer.Capacity);
			Assert.Equal(0, buffer.Length);
			Assert.Equal(0, buffer.Index);

			buffer.WriteInt32(1);
			buffer.WriteInt32(2);
			buffer.WriteInt32(3);
			Assert.Equal(10 + 256, buffer.Capacity);
			Assert.Equal(12, buffer.Length);
			Assert.Equal(12, buffer.Index);

			buffer.ResetLength();
			Assert.Equal(10 + 256, buffer.Capacity);
			Assert.Equal(0, buffer.Length);
			Assert.Equal(0, buffer.Index);
		}

		[Fact]
		public void ReadTo()
		{
			var buffer = new BufferReaderWriter(new byte[10], 0, 0, false);
			buffer.WriteByte(1);
			buffer.Seek(0, SeekOrigin.End);
			buffer.WriteByte(2);
			buffer.Seek(0, SeekOrigin.Begin);

			var arr = new byte[10];
			buffer.ReadTo(arr, 0, 10);
			Assert.Equal(new byte[] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, arr);

			Assert.Throws<InvalidOperationException>(() => buffer.ReadTo(arr, -1, 0));
			Assert.Throws<InvalidOperationException>(() => buffer.ReadTo(arr, 0, -1));

			arr = new byte[9];
			Assert.Throws<InvalidOperationException>(() => buffer.ReadTo(arr, 0, 10));

			arr = new byte[13];
			buffer.Seek(0, SeekOrigin.Begin);
			buffer.ReadTo(arr, 2, 11);
			Assert.Equal(new byte[] { 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2 }, arr);
		}

		[Fact]
		public void IndexOf()
		{
			var foobarBytes = Encoding.UTF8.GetBytes("foobar");

			var buffer = new BufferReaderWriter();
			buffer.WriteUInt16(0xFFFF);
			buffer.Write(foobarBytes);
			buffer.WriteByte(0);
			buffer.WriteUInt16(0xFFFF);

			buffer.Seek(0, SeekOrigin.Begin);
			Assert.Equal(0xFFFF, buffer.ReadUInt16());
			Assert.Equal(foobarBytes, buffer.Read(foobarBytes.Length));
			Assert.Equal(0, buffer.ReadByte());
			Assert.Equal(0xFFFF, buffer.ReadUInt16());

			buffer.Seek(0, SeekOrigin.Begin);
			Assert.Equal(8, buffer.IndexOf(0));
			Assert.Equal(8, buffer.IndexOf(0, 0));
			Assert.Equal(8, buffer.IndexOf(0, 1));
			Assert.Equal(8, buffer.IndexOf(0, 2));
			Assert.Equal(8, buffer.IndexOf(0, 6));
			Assert.Equal(8, buffer.IndexOf(0, 7));
			Assert.Equal(8, buffer.IndexOf(0, 8));
			Assert.Equal(-1, buffer.IndexOf(0, 9));

			buffer.Seek(0, SeekOrigin.Begin);
			Assert.Equal(0xFFFF, buffer.ReadUInt16());

			var index = buffer.IndexOf(0);
			Assert.Equal(8, index);
			var len = index - buffer.Index;
			Assert.Equal(6, len);
			var str = Encoding.UTF8.GetString(buffer.Read(len));
			Assert.Equal("foobar", str);
			Assert.Equal(0, buffer.ReadByte());
			Assert.Equal(0xFFFF, buffer.ReadUInt16());
		}
	}
}
