using System;
using System.Buffers;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;

namespace Yggdrasil.Util
{
	/// <summary>
	/// Reader and writer for a byte array.
	/// </summary>
	public class BufferReaderWriter : IDisposable
	{
		/// <summary>
		/// Default size for a new buffer.
		/// </summary>
		public const int DefaultSize = 128;

		/// <summary>
		/// Size added when the buffer runs out of space.
		/// </summary>
		public const int AddSize = 256;

		private byte[] _buffer;
		private int _ptr, _length, _minBufferLength;
		private bool _fixedLength;
		private bool _disposed;

		/// <summary>
		/// Returns the buffer's current position.
		/// </summary>
		public int Index => Math.Min(_ptr, this.Capacity);

		/// <summary>
		/// Returns the current length of the underlying array.
		/// </summary>
		public int Capacity => _minBufferLength;

		/// <summary>
		/// Returns the length of the actual data.
		/// </summary>
		public int Length => _length;

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
		/// <param name="length">Effective length of the data in the buffer.</param>
		public BufferReaderWriter(int length)
		{
			this.Reuse(length);
		}

		/// <summary>
		/// Creates a new buffer from byte array.
		/// </summary>
		/// <remarks>
		/// Copies buffer into an underlying and pooled array. The given
		/// buffer can be safely modified after creating this instance.
		/// </remarks>
		/// <param name="buffer">The buffer that is to be read from.</param>
		public BufferReaderWriter(ReadOnlySpan<byte> buffer)
		{
			this.Reuse(buffer);
		}

		/// <summary>
		/// Creates a new buffer from byte array, setting the index and
		/// the target length of the buffer accordingly.
		/// </summary>
		/// <remarks>
		/// Copies buffer into an underlying and pooled array. The given
		/// buffer can be safely modified after creating this instance.
		/// </remarks>
		/// <param name="buffer">The buffer that is to be read from.</param>
		/// <param name="index">Index to start reading from.</param>
		/// <param name="length">Effective length of the data in the buffer.</param>
		/// <param name="fixedLength">Indicates whether the buffer length is fixed or can be grown.</param>
		public BufferReaderWriter(ReadOnlySpan<byte> buffer, int index, int length, bool fixedLength)
		{
			this.Reuse(buffer, index, length, fixedLength);
		}

		/// <summary>
		/// Resets the instance to make it reusable for a writing session
		/// with the given length.
		/// </summary>
		/// <param name="length"></param>
		/// <exception cref="InvalidOperationException"></exception>
		public void Reuse(int length)
		{
			this.AssertNotDisposed();

			if (length < 0)
				throw new InvalidOperationException("Length must be a positive number.");

			if (_buffer != null)
				ArrayPool<byte>.Shared.Return(_buffer);

			_buffer = ArrayPool<byte>.Shared.Rent(length);
			_ptr = 0;
			_length = 0;
			_minBufferLength = length;
			_fixedLength = false;
		}

		/// <summary>
		/// Resets the instance to make it reusable for a reading session
		/// with the given buffer.
		/// </summary>
		/// <param name="buffer"></param>
		/// <exception cref="InvalidOperationException"></exception>
		public void Reuse(ReadOnlySpan<byte> buffer)
		{
			this.Reuse(buffer, 0, buffer.Length, false);
		}

		/// <summary>
		/// Resets the instance to make it reusable for a reading session
		/// with the given buffer.
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="index"></param>
		/// <param name="length"></param>
		/// <param name="fixedLength"></param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public void Reuse(ReadOnlySpan<byte> buffer, int index, int length, bool fixedLength)
		{
			this.AssertNotDisposed();

			if (buffer == null)
				throw new ArgumentNullException(nameof(buffer));

			if (index < 0)
				throw new InvalidOperationException("Index must be a positive number.");

			if (length < 0)
				throw new InvalidOperationException("Length must be a positive number.");

			if (buffer.Length < index + length)
				throw new ArgumentOutOfRangeException(nameof(buffer), "Buffer is not long enough.");

			if (_buffer != null)
				ArrayPool<byte>.Shared.Return(_buffer);

			_buffer = ArrayPool<byte>.Shared.Rent(buffer.Length);
			_ptr = index;
			_length = length;
			_minBufferLength = buffer.Length;
			_fixedLength = fixedLength;

			buffer.CopyTo(_buffer);
		}

		/// <summary>
		/// Cleans up resources used by this instance.
		/// </summary>
		public void Dispose()
		{
			if (_disposed)
				return;

			_disposed = true;

			ArrayPool<byte>.Shared.Return(_buffer);

			_buffer = null;
			_ptr = 0;
			_length = 0;
			_minBufferLength = 0;
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
			var requiredLength = _ptr + needed;

			if (requiredLength <= _minBufferLength)
				return;

			if (_fixedLength)
				throw new InvalidOperationException("Buffer can't be extended, as its length is fixed.");

			var add = Math.Max(needed, AddSize);
			var newLength = _minBufferLength + add;

			if (newLength > _buffer.Length)
			{
				var newBuffer = ArrayPool<byte>.Shared.Rent(newLength);
				Buffer.BlockCopy(_buffer, 0, newBuffer, 0, _minBufferLength);

				ArrayPool<byte>.Shared.Return(_buffer);
				_buffer = newBuffer;
			}

			_minBufferLength = newLength;
		}

		/// <summary>
		/// Throws InvalidOperationException if there's not enough bytes
		/// from the current position till the end to read the given
		/// amout of bytes.
		/// </summary>
		/// <param name="needed"></param>
		private void AssertEnoughBytes(int needed)
		{
			var requiredLength = _ptr + needed;

			if (requiredLength > _length)
				throw new InvalidOperationException("End of data in buffer.");
		}

		/// <summary>
		/// Throws ObjectDisposedException if this instance was disposed.
		/// </summary>
		/// <exception cref="ObjectDisposedException"></exception>
		private void AssertNotDisposed()
		{
			if (_disposed)
				throw new ObjectDisposedException("BufferReaderWriter");
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
			this.AssertNotDisposed();

			var result = new byte[_length];
			Buffer.BlockCopy(_buffer, 0, result, 0, _length);
			return result;
		}

		/// <summary>
		/// Copies the buffer's data into the given array, at the offset.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="offset"></param>
		public void CopyTo(Span<byte> destination, int offset)
			=> this.CopyTo(destination, offset, 0);

		/// <summary>
		/// Copies the buffer's data into the given array, at the offset.
		/// </summary>
		/// <param name="destination">The array to copy to.</param>
		/// <param name="destinationOffset">The offset in the array copied to.</param>
		/// <param name="sourceOffset">The offset from which to read in the buffer.</param>
		public void CopyTo(Span<byte> destination, int destinationOffset, int sourceOffset)
		{
			this.AssertNotDisposed();

			if (destinationOffset < 0)
				throw new InvalidOperationException("Offset must be a positive number.");

			if (destination.Length < _length + destinationOffset - sourceOffset)
				throw new InvalidOperationException("Destination is not long enough.");

			_buffer.AsSpan(sourceOffset, _length - sourceOffset).CopyTo(destination.Slice(destinationOffset));
		}

		/// <summary>
		/// Sets current index based on the given parameters.
		/// </summary>
		/// <param name="index">Modifier.</param>
		/// <param name="origin">Origin of the search.</param>
		public void Seek(int index, SeekOrigin origin)
		{
			this.AssertNotDisposed();

			switch (origin)
			{
				case SeekOrigin.Begin:
					if (index < 0 || index > _minBufferLength)
						throw new InvalidOperationException("Out of data in buffer.");
					_ptr = index;
					break;

				case SeekOrigin.End:
					if (index > 0 || -index > _minBufferLength)
						throw new InvalidOperationException("Out of data in buffer.");
					_ptr = _minBufferLength + index;
					break;

				case SeekOrigin.Current:
					if (_ptr + index < 0 || _ptr + index > _minBufferLength)
						throw new InvalidOperationException("Out of data in buffer.");
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
			this.AssertNotDisposed();

			_ptr = 0;
			_length = 0;
		}

		/// <summary>
		/// Searches for the given byte in the buffer, starting at the
		/// current position, and returns the index of the first occurence.
		/// If the byte is not found, -1 is returned.
		/// </summary>
		/// <example>
		/// var buffer = new Buffer(Encoding.ASCII.GetBytes("foobar"));
		/// buffer.Find((byte)'b'); // 3
		/// </example>
		/// <param name="val">Value to search for.</param>
		/// <returns></returns>
		public int IndexOf(byte val)
			=> this.IndexOf(val, this.Index);

		/// <summary>
		/// Searches for the given byte in the buffer, starting at the
		/// given position, and returns the index of the first occurence.
		/// If the byte is not found, -1 is returned.
		/// </summary>
		/// <example>
		/// var buffer = new Buffer(Encoding.ASCII.GetBytes("foobar"), 0);
		/// buffer.Find((byte)'b'); // 3
		/// 
		/// var buffer = new Buffer(Encoding.ASCII.GetBytes("foobar"), 4);
		/// buffer.Find((byte)'b'); // -1
		/// </example>
		/// <param name="val">Value to search for.</param>
		/// <param name="startIndex">Index to start at.</param>
		/// <returns></returns>
		public int IndexOf(byte val, int startIndex)
		{
			this.AssertNotDisposed();

			for (var i = startIndex; i < _length; ++i)
			{
				if (_buffer[i] == val)
					return i;
			}

			return -1;
		}

		// Reading
		// ------------------------------------------------------------------

		/// <summary>
		/// Returns the current byte without increasing the index.
		/// </summary>
		/// <returns></returns>
		public byte Peek()
		{
			this.AssertNotDisposed();

			return _buffer[_ptr];
		}

		/// <summary>
		/// Returns the byte at the given position without modifying the
		/// index.
		/// </summary>
		/// <returns></returns>
		public byte GetAt(int index)
		{
			this.AssertNotDisposed();

			if (index < 0 || index > _minBufferLength - 1)
				throw new InvalidOperationException("Out of data in buffer.");

			return _buffer[index];
		}

		/// <summary>
		/// Returns the next byte in the buffer.
		/// </summary>
		/// <returns></returns>
		public byte ReadByte()
		{
			this.AssertNotDisposed();
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
			this.AssertNotDisposed();
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
			this.AssertNotDisposed();
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
			this.AssertNotDisposed();
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
			this.AssertNotDisposed();
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
			this.AssertNotDisposed();
			this.AssertEnoughBytes(sizeof(float));

			var intValue = 0;
			for (var i = 0; i < sizeof(float); ++i)
				intValue += ((int)_buffer[_ptr++] << (i * 8));

			if (this.Endianness == Endianness.BigEndian)
				intValue = IPAddress.NetworkToHostOrder(intValue);

			return new FloatIntUnion { IntValue = intValue }.FloatValue;
		}

		/// <summary>
		/// Returns the next double in the buffer.
		/// </summary>
		/// <returns></returns>
		public double ReadDouble()
		{
			this.AssertNotDisposed();
			this.AssertEnoughBytes(sizeof(double));

			var longValue = 0L;
			for (var i = 0; i < sizeof(double); ++i)
				longValue += ((long)_buffer[_ptr++] << (i * 8));

			if (this.Endianness == Endianness.BigEndian)
				longValue = IPAddress.NetworkToHostOrder(longValue);

			return new DoubleLongUnion { LongValue = longValue }.DoubleValue;
		}

		/// <summary>
		/// Returns the next x bytes in the buffer.
		/// </summary>
		/// <returns></returns>
		public byte[] Read(int length)
		{
			this.AssertNotDisposed();
			this.AssertEnoughBytes(length);

			if (length < 0)
				throw new InvalidOperationException("Length must be a positive number.");

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
		public void ReadTo(Span<byte> buffer, int offset, int length)
		{
			this.AssertNotDisposed();

			if (offset < 0)
				throw new InvalidOperationException("Offset must be a positive number.");

			if (length < 0)
				throw new InvalidOperationException("Length must be a positive number.");

			if (buffer.Length < length + offset)
				throw new InvalidOperationException("Destination is not long enough.");

			this.AssertEnoughBytes(length);

			_buffer.AsSpan(_ptr, length).CopyTo(buffer.Slice(offset, length));
			_ptr += length;
		}

		/// <summary>
		/// Returns the next x bytes in the buffer as a span.
		/// </summary>
		/// <remarks>
		/// The span is valid for as long as the buffer isn't modified or
		/// disposed.
		/// </remarks>
		/// <param name="length"></param>
		/// <returns></returns>
		public ReadOnlySpan<byte> ReadAsSpan(int length)
		{
			this.AssertNotDisposed();
			this.AssertEnoughBytes(length);

			if (length < 0)
				throw new InvalidOperationException("Length must be a positive number.");

			var result = new ReadOnlySpan<byte>(_buffer, _ptr, length);
			_ptr += length;

			return result;
		}

		// Writing
		// ------------------------------------------------------------------

		/// <summary>
		/// Writes value to buffer.
		/// </summary>
		/// <param name="value"></param>
		public void WriteByte(byte value)
		{
			this.AssertNotDisposed();
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
			this.AssertNotDisposed();
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
		/// Writes 3-byte integer value to buffer.
		/// </summary>
		/// <param name="value"></param>
		public void WriteInt24(int value)
		{
			this.AssertNotDisposed();
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
			this.AssertNotDisposed();
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
		public void WriteInt32(int value) => this.WriteInt32(value, this.Endianness);

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
			this.AssertNotDisposed();
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
			this.AssertNotDisposed();
			this.EnsureSpace(sizeof(float));

			var intValue = new FloatIntUnion { FloatValue = value }.IntValue;

			if (this.Endianness == Endianness.BigEndian)
				intValue = IPAddress.HostToNetworkOrder(intValue);

			for (var i = 0; i < sizeof(float); ++i)
				_buffer[_ptr + i] = (byte)((intValue >> (i * 8)) & 0xFF);
			this.UpdatePtrLength(sizeof(float));
		}

		/// <summary>
		/// Writes value to buffer.
		/// </summary>
		/// <param name="value"></param>
		public void WriteDouble(double value)
		{
			this.AssertNotDisposed();
			this.EnsureSpace(sizeof(double));

			var longValue = new DoubleLongUnion { DoubleValue = value }.LongValue;

			if (this.Endianness == Endianness.BigEndian)
				longValue = IPAddress.HostToNetworkOrder(longValue);

			for (var i = 0; i < sizeof(double); ++i)
				_buffer[_ptr + i] = (byte)((longValue >> (i * 8)) & 0xFF);
			this.UpdatePtrLength(sizeof(double));
		}

		/// <summary>
		/// Writes value to buffer.
		/// </summary>
		/// <param name="value"></param>
		public void Write(ReadOnlySpan<byte> value) => this.Write(value, 0, value.Length);

		/// <summary>
		/// Writes value to buffer, starting at index for the given length.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="index"></param>
		/// <param name="length"></param>
		public void Write(ReadOnlySpan<byte> value, int index, int length)
		{
			this.AssertNotDisposed();
			this.EnsureSpace(length);

			if (index < 0)
				throw new ArgumentException("Index must be a positive number.", nameof(index));

			if (length < 0)
				throw new ArgumentException("Length must be a positive number.", nameof(length));

			if (value.Length < index + length)
				throw new ArgumentException("Value is not long enough.", nameof(value));

			value.Slice(index, length).CopyTo(_buffer.AsSpan(_ptr, length));
			this.UpdatePtrLength(length);
		}

		[StructLayout(LayoutKind.Explicit)]
		private struct FloatIntUnion
		{
			[FieldOffset(0)]
			public int IntValue;

			[FieldOffset(0)]
			public float FloatValue;
		}

		[StructLayout(LayoutKind.Explicit)]
		private struct DoubleLongUnion
		{
			[FieldOffset(0)]
			public long LongValue;

			[FieldOffset(0)]
			public double DoubleValue;
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
