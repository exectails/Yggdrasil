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
	}
}
