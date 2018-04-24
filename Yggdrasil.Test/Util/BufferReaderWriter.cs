// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.IO;
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
			buffer.WriteShort(0x4243);
			Assert.Equal(new byte[] { 0x42, 0x43 }, buffer.Copy());

			buffer = new BufferReaderWriter();
			buffer.WriteInt(0x42434445);
			Assert.Equal(new byte[] { 0x42, 0x43, 0x44, 0x45 }, buffer.Copy());

			buffer = new BufferReaderWriter();
			buffer.WriteLong(0x4243444546474849);
			Assert.Equal(new byte[] { 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49 }, buffer.Copy());

			buffer = new BufferReaderWriter();
			buffer.WriteFloat(42.43f);
			Assert.Equal(new byte[] { 0x52, 0xB8, 0x29, 0x42 }, buffer.Copy());

			buffer = new BufferReaderWriter();
			buffer.WriteDouble(42.43);
			Assert.Equal(new byte[] { 0xD7, 0xA3, 0x70, 0x3D, 0x0A, 0x37, 0x45, 0x40 }, buffer.Copy());

			buffer = new BufferReaderWriter();
			buffer.Write(new byte[] { 0x42, 0x43, 0x44, 0x45 });
			Assert.Equal(new byte[] { 0x42, 0x43, 0x44, 0x45 }, buffer.Copy());
		}

		[Fact]
		public void Reading()
		{
			var buffer = new BufferReaderWriter(31);
			buffer.WriteByte(0x42);
			buffer.WriteShort(0x4243);
			buffer.WriteInt(0x42434445);
			buffer.WriteLong(0x4243444546474849);
			buffer.WriteFloat(42.43f);
			buffer.WriteDouble(42.43);
			buffer.Write(new byte[] { 0x42, 0x43, 0x44, 0x45 });

			buffer.Seek(0, SeekOrigin.Begin);

			Assert.Equal(0x42, buffer.ReadByte());
			Assert.Equal(0x4243, buffer.ReadShort());
			Assert.Equal(0x42434445, buffer.ReadInt());
			Assert.Equal(0x4243444546474849, buffer.ReadLong());
			Assert.Equal(42.43f, buffer.ReadFloat());
			Assert.Equal(42.43, buffer.ReadDouble());
			Assert.Equal(new byte[] { 0x42, 0x43, 0x44, 0x45 }, buffer.Read(4));

			Assert.Throws<InvalidOperationException>(() => { buffer.Read(1); });
		}

		[Fact]
		public void Seeking()
		{
			var buffer = new BufferReaderWriter(10);

			buffer.Seek(0, SeekOrigin.Begin);
			Assert.Equal(0, buffer.Index);

			buffer.Seek(0, SeekOrigin.End);
			Assert.Equal(9, buffer.Index);

			buffer.Seek(-4, SeekOrigin.Current);
			Assert.Equal(5, buffer.Index);

			buffer.Seek(2, SeekOrigin.Current);
			Assert.Equal(7, buffer.Index);

			Assert.DoesNotThrow(() => { buffer.Seek(2, SeekOrigin.Current); });
			buffer.Seek(-2, SeekOrigin.Current);
			Assert.Throws<InvalidOperationException>(() => { buffer.Seek(3, SeekOrigin.Current); });

			Assert.DoesNotThrow(() => { buffer.Seek(-7, SeekOrigin.Current); });
			buffer.Seek(2, SeekOrigin.Current);
			Assert.Throws<InvalidOperationException>(() => { buffer.Seek(-3, SeekOrigin.Current); });

			Assert.DoesNotThrow(() => { buffer.Seek(-1, SeekOrigin.End); });
			Assert.DoesNotThrow(() => { buffer.Seek(-5, SeekOrigin.End); });
			Assert.DoesNotThrow(() => { buffer.Seek(-9, SeekOrigin.End); });
			Assert.Throws<InvalidOperationException>(() => { buffer.Seek(1, SeekOrigin.End); });
			Assert.Throws<InvalidOperationException>(() => { buffer.Seek(10, SeekOrigin.End); });

			Assert.DoesNotThrow(() => { buffer.Seek(1, SeekOrigin.Begin); });
			Assert.DoesNotThrow(() => { buffer.Seek(5, SeekOrigin.Begin); });
			Assert.DoesNotThrow(() => { buffer.Seek(9, SeekOrigin.Begin); });
			Assert.Throws<InvalidOperationException>(() => { buffer.Seek(-1, SeekOrigin.Begin); });
			Assert.Throws<InvalidOperationException>(() => { buffer.Seek(10, SeekOrigin.Begin); });

			buffer.Seek(5, SeekOrigin.Begin);
			Assert.DoesNotThrow(() => { buffer.Seek(1, SeekOrigin.Current); });
			buffer.Seek(5, SeekOrigin.Begin);
			Assert.DoesNotThrow(() => { buffer.Seek(4, SeekOrigin.Current); });
			buffer.Seek(5, SeekOrigin.Begin);
			Assert.DoesNotThrow(() => { buffer.Seek(-1, SeekOrigin.Current); });
			buffer.Seek(5, SeekOrigin.Begin);
			Assert.DoesNotThrow(() => { buffer.Seek(-5, SeekOrigin.Current); });
			buffer.Seek(5, SeekOrigin.Begin);
			Assert.Throws<InvalidOperationException>(() => { buffer.Seek(-6, SeekOrigin.Current); });
			buffer.Seek(5, SeekOrigin.Begin);
			Assert.Throws<InvalidOperationException>(() => { buffer.Seek(5, SeekOrigin.Current); });
		}

		[Fact]
		public void Resizing()
		{
			var buffer = new BufferReaderWriter(4);
			Assert.Equal(4, buffer.Capacity);

			buffer.WriteInt(0);
			Assert.Equal(4, buffer.Capacity);

			buffer.WriteInt(0);
			Assert.Equal(260, buffer.Capacity);
		}

		[Fact]
		public void LengthExtension()
		{
			var buffer = new BufferReaderWriter(100);

			buffer.WriteInt(0x01020304);
			Assert.Equal(4, buffer.Length);

			buffer.Seek(2, SeekOrigin.Begin);
			buffer.WriteByte(1);
			Assert.Equal(4, buffer.Length);
			Assert.Equal(new byte[] { 1, 2, 1, 4 }, buffer.Copy());

			buffer.Seek(3, SeekOrigin.Begin);
			buffer.WriteShort(0x0104);
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

			buffer.WriteInt(0x01020304);
			buffer.WriteInt(0x01020304);
			Assert.DoesNotThrow(() => buffer.WriteInt(0x01020304));

			buffer = new BufferReaderWriter(new byte[10], 0, 0, true);

			buffer.WriteInt(0x01020304);
			buffer.WriteInt(0x01020304);
			Assert.Throws<InvalidOperationException>(() => buffer.WriteInt(0x01020304));
		}

		[Fact]
		public void CopyTo()
		{
			var buffer = new BufferReaderWriter(new byte[10], 0, 0, false);
			buffer.WriteByte(1);
			buffer.Seek(0, SeekOrigin.End);
			buffer.WriteByte(2);

			var arr = new byte[10];
			buffer.CopyTo(arr, 0);
			Assert.Equal(new byte[] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 2 }, arr);

			Assert.Throws<InvalidOperationException>(() => buffer.CopyTo(arr, -1));

			arr = new byte[9];
			Assert.Throws<InvalidOperationException>(() => buffer.CopyTo(arr, 0));

			arr = new byte[12];
			buffer.CopyTo(arr, 2);
			Assert.Equal(new byte[] { 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 2 }, arr);
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
			Assert.Equal(new byte[] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 2 }, arr);

			Assert.Throws<InvalidOperationException>(() => buffer.ReadTo(arr, -1, 0));
			Assert.Throws<InvalidOperationException>(() => buffer.ReadTo(arr, 0, -1));

			arr = new byte[9];
			Assert.Throws<InvalidOperationException>(() => buffer.ReadTo(arr, 0, 10));

			arr = new byte[12];
			buffer.Seek(0, SeekOrigin.Begin);
			buffer.ReadTo(arr, 2, 10);
			Assert.Equal(new byte[] { 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 2 }, arr);
		}
	}
}
