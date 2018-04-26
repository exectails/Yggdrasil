// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

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
		private bool _fixedLength;

		/// <summary>
		/// Returns the buffer's current position.
		/// </summary>
		public int Index { get { return _ptr; } }

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
			if (offset < 0)
				throw new InvalidOperationException("Offset must be a positive number.");

			if (destination.Length < _length + offset)
				throw new InvalidOperationException("Destination is not long enough.");

			Buffer.BlockCopy(_buffer, 0, destination, offset, _length);
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
					if (index < 0 || index > _buffer.Length - 1)
						throw new InvalidOperationException("Out of buffer.");
					_ptr = index;
					break;

				case SeekOrigin.End:
					if (index > 0 || -index > _buffer.Length)
						throw new InvalidOperationException("Out of buffer.");
					_ptr = _buffer.Length - index - 1;
					break;

				case SeekOrigin.Current:
					if (_ptr + index < 0 || _ptr + index > _buffer.Length - 1)
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
		/// Returns the next short in the buffer.
		/// </summary>
		/// <returns></returns>
		public short ReadShort()
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
		/// Returns the next short in the buffer.
		/// </summary>
		/// <returns></returns>
		public ushort ReadUShort()
		{
			return (ushort)this.ReadShort();
		}

		/// <summary>
		/// Returns the next int in the buffer.
		/// </summary>
		/// <returns></returns>
		public int ReadInt()
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
		/// Returns the next int in the buffer.
		/// </summary>
		/// <returns></returns>
		public uint ReadUInt()
		{
			return (uint)this.ReadInt();
		}

		/// <summary>
		/// Returns the next long in the buffer.
		/// </summary>
		/// <returns></returns>
		public long ReadLong()
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
		/// Returns the next long in the buffer.
		/// </summary>
		/// <returns></returns>
		public ulong ReadULong()
		{
			return (ulong)this.ReadLong();
		}

		/// <summary>
		/// Returns the next float in the buffer.
		/// </summary>
		/// <returns></returns>
		public float ReadFloat()
		{
			this.AssertEnoughBytes(sizeof(float));

			var result = BitConverter.ToSingle(_buffer, _ptr);
			_ptr += sizeof(float);

			return result;
		}

		/// <summary>
		/// Returns the next double in the buffer.
		/// </summary>
		/// <returns></returns>
		public double ReadDouble()
		{
			this.AssertEnoughBytes(sizeof(double));

			var result = BitConverter.ToDouble(_buffer, _ptr);
			_ptr += sizeof(double);

			return result;
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
		public void WriteShort(short value)
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
		public void WriteUShort(ushort value)
		{
			this.WriteShort((short)value);
		}

		/// <summary>
		/// Writes value to buffer.
		/// </summary>
		/// <param name="value"></param>
		public void WriteInt(int value)
		{
			this.EnsureSpace(sizeof(int));

			if (this.Endianness == Endianness.BigEndian)
				value = IPAddress.HostToNetworkOrder(value);

			for (var i = 0; i < sizeof(int); ++i)
				_buffer[_ptr + i] = (byte)((value >> (i * 8)) & 0xFF);
			this.UpdatePtrLength(sizeof(int));
		}

		/// <summary>
		/// Writes value to buffer.
		/// </summary>
		/// <param name="value"></param>
		public void WriteUInt(uint value)
		{
			this.WriteInt((int)value);
		}

		/// <summary>
		/// Writes value to buffer.
		/// </summary>
		/// <param name="value"></param>
		public void WriteLong(long value)
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
		public void WriteULong(ulong value)
		{
			this.WriteLong((long)value);
		}

		/// <summary>
		/// Writes value to buffer.
		/// </summary>
		/// <param name="value"></param>
		public void WriteFloat(float value)
		{
			this.EnsureSpace(sizeof(float));

			var bytes = BitConverter.GetBytes(value);
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
			this.Write(bytes);
		}

		/// <summary>
		/// Writes value to buffer.
		/// </summary>
		/// <param name="value"></param>
		public void Write(byte[] value)
		{
			var length = value.Length;

			this.EnsureSpace(length);

			Buffer.BlockCopy(value, 0, _buffer, _ptr, length);
			this.UpdatePtrLength(length);
		}
	}

	/// <summary>
	/// Describes endiannes.
	/// </summary>
	public enum Endianness
	{
		/// <summary>
		/// Lower bits first.
		/// </summary>
		LittleEndian,

		/// <summary>
		/// Higher bits first.
		/// </summary>
		BigEndian,
	}
}
