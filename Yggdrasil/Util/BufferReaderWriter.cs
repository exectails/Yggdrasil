using System;
using System.IO;
using System.Net;

namespace Yggdrasil.Util
{
	/// <summary>
	/// Reader and writer for a byte array.
	/// </summary>
	public class BufferReaderWriter
	{
		/// <summary>
		/// Default size for a new buffer.
		/// </summary>
		private const int DefaultSize = 128;

		/// <summary>
		/// Size added every time the buffer runs out of space.
		/// </summary>
		private const int AddSize = 256;

		private byte[] _buffer;
		private int _ptr, _length;
		private readonly bool _fixedLength;

		/// <summary>
		/// Returns the buffer's current position.
		/// </summary>
		public int Index { get { return Math.Min(_ptr, this.Capacity); } }

		/// <summary>
		/// Returns the current length of the underlying array.
		/// </summary>
		public int Capacity { get { return _buffer.Length; } }

		/// <summary>
		/// Returns the length of the actual data.
		/// </summary>
		public int Length { get { return _length; } }

		/// <summary>
		/// Gets or sets this instance's endianness.
		/// </summary>
		public Endianness Endianness { get; set; } = Endianness.BigEndian;

		/// <summary>
		/// Creates new buffer with default size.
		/// </summary>
		public BufferReaderWriter()
			: this(DefaultSize)
		{
		}

		/// <summary>
		/// Creates a new buffer with the given length.
		/// </summary>
		/// <param name="length"></param>
		public BufferReaderWriter(int length)
			: this(new byte[length], 0, 0, false)
		{
		}

		/// <summary>
		/// Creates a new buffer from byte array.
		/// </summary>
		/// <param name="buffer"></param>
		public BufferReaderWriter(byte[] buffer)
			: this(buffer, 0, buffer.Length, false)
		{
		}

		/// <summary>
		/// Creates a new buffer from byte array, setting the index and
		/// the target length of the buffer accordingly.
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="index"></param>
		/// <param name="length"></param>
		/// <param name="fixedLength"></param>
		public BufferReaderWriter(byte[] buffer, int index, int length, bool fixedLength)
		{
			_buffer = buffer;
			_ptr = index;
			_length = length;
			_fixedLength = fixedLength;
		}

		// General
		// ------------------------------------------------------------------

		/// <summary>
		/// Increases the underlying array's size if necessary to add the
		/// given amount of bytes from the current position.
		/// </summary>
		/// <param name="needed"></param>
		private void EnsureSpace(int needed)
		{
			if (_ptr + needed <= _buffer.Length)
				return;

			if (_fixedLength)
				throw new InvalidOperationException("Buffer can't be extended, as its length is fixed.");

			var add = Math.Max(needed, AddSize);
			Array.Resize(ref _buffer, _buffer.Length + add);
		}

		/// <summary>
		/// Throws InvalidOperationException if there's not enough bytes
		/// from the current position till the end to read the given
		/// amout of bytes.
		/// </summary>
		/// <param name="needed"></param>
		private void AssertEnoughBytes(int needed)
		{
			if (_ptr + needed > _buffer.Length)
				throw new InvalidOperationException("End of buffer.");
		}

		/// <summary>
		/// Returns a copy of the buffer, limited to the actual values.
		/// </summary>
		/// <remarks>
		/// If a buffer with a size of 100, that was not created from an
		/// existing array, gets copied after one int was written to it,
		/// the result will only be four byte long.
		/// </remarks>
		/// <returns></returns>
		public byte[] Copy()
		{
			var result = new byte[_length];
			Buffer.BlockCopy(_buffer, 0, result, 0, _length);
			return result;
		}

		/// <summary>
		/// Copies the buffer's data into the given array, at the offset.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="offset"></param>
		public void CopyTo(byte[] destination, int offset)
		{
			this.CopyTo(destination, offset, 0);
		}

		/// <summary>
		/// Copies the buffer's data into the given array, at the offset.
		/// </summary>
		/// <param name="destination">The array to copy to.</param>
		/// <param name="destinationOffset">The offset in the array copied to.</param>
		/// <param name="sourceOffset">The offset from which to read in the buffer.</param>
		public void CopyTo(byte[] destination, int destinationOffset, int sourceOffset)
		{
			if (destinationOffset < 0)
				throw new InvalidOperationException("Offset must be a positive number.");

			if (destination.Length < _length + destinationOffset - sourceOffset)
				throw new InvalidOperationException("Destination is not long enough.");

			Buffer.BlockCopy(_buffer, sourceOffset, destination, destinationOffset, _length - sourceOffset);
		}

		/// <summary>
		/// Sets current index based on the given parameters.
		/// </summary>
		/// <param name="index">Modifier.</param>
		/// <param name="origin">Origin of the search.</param>
		public void Seek(int index, SeekOrigin origin)
		{
			switch (origin)
			{
				case SeekOrigin.Begin:
					if (index < 0 || index > _buffer.Length)
						throw new InvalidOperationException("Out of buffer.");
					_ptr = index;
					break;

				case SeekOrigin.End:
					if (index > 0 || -index > _buffer.Length)
						throw new InvalidOperationException("Out of buffer.");
					_ptr = _buffer.Length + index;
					break;

				case SeekOrigin.Current:
					if (_ptr + index < 0 || _ptr + index > _buffer.Length)
						throw new InvalidOperationException("Out of buffer.");
					_ptr += index;
					break;

				default:
					throw new InvalidOperationException("Unsupported origin.");
			}
		}

		/// <summary>
		/// Updates index and length, so it coresponds with the actual
		/// length of the data, based on the current index.
		/// </summary>
		/// <param name="mod"></param>
		private void UpdatePtrLength(int mod)
		{
			var diff = (_ptr + mod) - _length;
			if (diff > 0)
				_length += diff;

			_ptr += mod;
		}

		/// <summary>
		/// Resets the length of the underlying buffer, making the length
		/// of the actual data 0.
		/// </summary>
		public void ResetLength()
		{
			_ptr = 0;
			_length = 0;
		}

		/// <summary>
		/// Reverses the order of the bytes.
		/// </summary>
		/// <param name="bytes"></param>
		private static void ReverseBytes(ref byte[] bytes)
		{
			Array.Reverse(bytes, 0, bytes.Length);
		}

		// Reading
		// ------------------------------------------------------------------

		/// <summary>
		/// Returns the current byte without increasing the index.
		/// </summary>
		/// <returns></returns>
		public byte Peek()
		{
			return _buffer[_ptr];
		}

		/// <summary>
		/// Returns the byte at the given position without modifying the
		/// index.
		/// </summary>
		/// <returns></returns>
		public byte GetAt(int index)
		{
			if (index < 0 || index > _buffer.Length - 1)
				throw new InvalidOperationException("Out of buffer.");

			return _buffer[index];
		}

		/// <summary>
		/// Returns the next byte in the buffer.
		/// </summary>
		/// <returns></returns>
		public byte ReadByte()
		{
			this.AssertEnoughBytes(sizeof(byte));

			var result = _buffer[_ptr++];
			return result;
		}

		/// <summary>
		/// Returns the next signed byte in the buffer.
		/// </summary>
		/// <returns></returns>
		public sbyte ReadSByte() => (sbyte)this.ReadByte();

		/// <summary>
		/// Returns the next signed short in the buffer.
		/// </summary>
		/// <returns></returns>
		public short ReadInt16()
		{
			this.AssertEnoughBytes(sizeof(short));

			var result = 0;
			for (var i = 0; i < sizeof(short); ++i)
				result += ((short)_buffer[_ptr++] << (i * 8));

			if (this.Endianness == Endianness.BigEndian)
				return IPAddress.NetworkToHostOrder((short)result);
			else
				return (short)result;
		}

		/// <summary>
		/// Returns the next unsigned short in the buffer.
		/// </summary>
		/// <returns></returns>
		public ushort ReadUInt16() => (ushort)this.ReadInt16();

		/// <summary>
		/// Returns the next signed int in the buffer.
		/// </summary>
		/// <returns></returns>
		public int ReadInt32()
		{
			this.AssertEnoughBytes(sizeof(int));

			var result = 0;
			for (var i = 0; i < sizeof(int); ++i)
				result += ((int)_buffer[_ptr++] << (i * 8));

			if (this.Endianness == Endianness.BigEndian)
				return IPAddress.NetworkToHostOrder(result);
			else
				return result;
		}

		/// <summary>
		/// Returns the next unsigned int in the buffer.
		/// </summary>
		/// <returns></returns>
		public uint ReadUInt32() => (uint)this.ReadInt32();

		/// <summary>
		/// Returns the next 3-byte integer in the buffer.
		/// </summary>
		/// <returns></returns>
		public int ReadInt24()
		{
			this.AssertEnoughBytes(sizeof(int) - 1);

			var result = 0;
			if (this.Endianness == Endianness.BigEndian)
			{
				result |= (_buffer[_ptr++] << 16);
				result |= (_buffer[_ptr++] << 8);
				result |= (_buffer[_ptr++] << 0);
			}
			else
			{
				result |= (_buffer[_ptr++] << 0);
				result |= (_buffer[_ptr++] << 8);
				result |= (_buffer[_ptr++] << 16);
			}

			return result;
		}

		/// <summary>
		/// Returns the next signed long in the buffer.
		/// </summary>
		/// <returns></returns>
		public long ReadInt64()
		{
			this.AssertEnoughBytes(sizeof(long));

			var result = 0L;
			for (var i = 0; i < sizeof(long); ++i)
				result += ((long)_buffer[_ptr++] << (i * 8));

			if (this.Endianness == Endianness.BigEndian)
				return IPAddress.NetworkToHostOrder(result);
			else
				return result;
		}

		/// <summary>
		/// Returns the next unsigned long in the buffer.
		/// </summary>
		/// <returns></returns>
		public ulong ReadUInt64() => (ulong)this.ReadInt64();

		/// <summary>
		/// Returns the next float in the buffer.
		/// </summary>
		/// <returns></returns>
		public float ReadFloat()
		{
			var bytes = this.Read(sizeof(float));
			if (this.Endianness == Endianness.BigEndian && BitConverter.IsLittleEndian)
				ReverseBytes(ref bytes);

			return BitConverter.ToSingle(bytes, 0);
		}

		/// <summary>
		/// Returns the next double in the buffer.
		/// </summary>
		/// <returns></returns>
		public double ReadDouble()
		{
			var bytes = this.Read(sizeof(double));
			if (this.Endianness == Endianness.BigEndian && BitConverter.IsLittleEndian)
				ReverseBytes(ref bytes);

			return BitConverter.ToDouble(bytes, 0);
		}

		/// <summary>
		/// Returns the next x bytes in the buffer.
		/// </summary>
		/// <returns></returns>
		public byte[] Read(int length)
		{
			this.AssertEnoughBytes(length);

			var result = new byte[length];
			Buffer.BlockCopy(_buffer, _ptr, result, 0, length);
			_ptr += length;

			return result;
		}

		/// <summary>
		/// Reads the specified number of bytes and writes them into
		/// the buffer, at the given offset.
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		public void ReadTo(byte[] buffer, int offset, int length)
		{
			if (offset < 0)
				throw new InvalidOperationException("Offset must be a positive number.");

			if (length < 0)
				throw new InvalidOperationException("Length must be a positive number.");

			if (buffer.Length < length + offset)
				throw new InvalidOperationException("Destination is not long enough.");

			this.AssertEnoughBytes(length);

			Buffer.BlockCopy(_buffer, _ptr, buffer, offset, length);
			_ptr += length;
		}

		// Writing
		// ------------------------------------------------------------------

		/// <summary>
		/// Writes value to buffer.
		/// </summary>
		/// <param name="value"></param>
		public void WriteByte(byte value)
		{
			this.EnsureSpace(sizeof(byte));

			_buffer[_ptr] = value;
			this.UpdatePtrLength(sizeof(byte));
		}

		/// <summary>
		/// Writes value to buffer.
		/// </summary>
		/// <param name="value"></param>
		public void WriteSByte(sbyte value) => this.WriteByte((byte)value);

		/// <summary>
		/// Writes value to buffer.
		/// </summary>
		/// <param name="value"></param>
		public void WriteInt16(short value)
		{
			this.EnsureSpace(sizeof(short));

			if (this.Endianness == Endianness.BigEndian)
				value = IPAddress.HostToNetworkOrder(value);

			for (var i = 0; i < sizeof(short); ++i)
				_buffer[_ptr + i] = (byte)((value >> (i * 8)) & 0xFF);
			this.UpdatePtrLength(sizeof(short));
		}

		/// <summary>
		/// Writes value to buffer.
		/// </summary>
		/// <param name="value"></param>
		public void WriteUInt16(ushort value) => this.WriteInt16((short)value);

		/// <summary>
		/// Writes value to buffer.
		/// </summary>
		/// <param name="value"></param>
		public void WriteInt32(int value) => this.WriteInt32(value, this.Endianness);

		/// <summary>
		/// Writes 3-byte integer value to buffer.
		/// </summary>
		/// <param name="value"></param>
		public void WriteInt24(int value)
		{
			this.AssertEnoughBytes(sizeof(int) - 1);

			if (this.Endianness == Endianness.BigEndian)
			{
				_buffer[_ptr + 0] = (byte)(value >> 16);
				_buffer[_ptr + 1] = (byte)(value >> 8);
				_buffer[_ptr + 2] = (byte)(value >> 0);
			}
			else
			{
				_buffer[_ptr + 0] = (byte)(value >> 0);
				_buffer[_ptr + 1] = (byte)(value >> 8);
				_buffer[_ptr + 2] = (byte)(value >> 16);
			}

			this.UpdatePtrLength(sizeof(int) - 1);
		}

		/// <summary>
		/// Writes value to buffer.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="endianness"></param>
		public void WriteInt32(int value, Endianness endianness)
		{
			this.EnsureSpace(sizeof(int));

			if (endianness == Endianness.BigEndian)
				value = IPAddress.HostToNetworkOrder(value);

			for (var i = 0; i < sizeof(int); ++i)
				_buffer[_ptr + i] = (byte)((value >> (i * 8)) & 0xFF);
			this.UpdatePtrLength(sizeof(int));
		}

		/// <summary>
		/// Writes value to buffer.
		/// </summary>
		/// <param name="value"></param>
		public void WriteUInt32(uint value) => this.WriteInt32((int)value);

		/// <summary>
		/// Writes value to buffer.
		/// </summary>
		/// <param name="value"></param>
		public void WriteInt64(long value)
		{
			this.EnsureSpace(sizeof(long));

			if (this.Endianness == Endianness.BigEndian)
				value = IPAddress.HostToNetworkOrder(value);

			for (var i = 0; i < sizeof(long); ++i)
				_buffer[_ptr + i] = (byte)((value >> (i * 8)) & 0xFF);
			this.UpdatePtrLength(sizeof(long));
		}

		/// <summary>
		/// Writes value to buffer.
		/// </summary>
		/// <param name="value"></param>
		public void WriteUInt64(ulong value) => this.WriteInt64((long)value);

		/// <summary>
		/// Writes value to buffer.
		/// </summary>
		/// <param name="value"></param>
		public void WriteFloat(float value)
		{
			this.EnsureSpace(sizeof(float));

			var bytes = BitConverter.GetBytes(value);
			if (this.Endianness == Endianness.BigEndian && BitConverter.IsLittleEndian)
				ReverseBytes(ref bytes);

			this.Write(bytes);
		}

		/// <summary>
		/// Writes value to buffer.
		/// </summary>
		/// <param name="value"></param>
		public void WriteDouble(double value)
		{
			this.EnsureSpace(sizeof(double));

			var bytes = BitConverter.GetBytes(value);
			if (this.Endianness == Endianness.BigEndian && BitConverter.IsLittleEndian)
				ReverseBytes(ref bytes);

			this.Write(bytes);
		}

		/// <summary>
		/// Writes value to buffer.
		/// </summary>
		/// <param name="value"></param>
		public void Write(byte[] value) => this.Write(value, 0, value.Length);

		/// <summary>
		/// Writes value to buffer, starting at index for the given length.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="index"></param>
		/// <param name="length"></param>
		public void Write(byte[] value, int index, int length)
		{
			this.EnsureSpace(length);

			Buffer.BlockCopy(value, index, _buffer, _ptr, length);
			this.UpdatePtrLength(length);
		}
	}

	/// <summary>
	/// Describes endianness.
	/// </summary>
	public enum Endianness
	{
		/// <summary>
		/// Lower bits first, how most modern systems store data in memory.
		/// </summary>
		LittleEndian,

		/// <summary>
		/// Higher bits first, how we commenly display hex values.
		/// </summary>
		BigEndian,
	}
}
