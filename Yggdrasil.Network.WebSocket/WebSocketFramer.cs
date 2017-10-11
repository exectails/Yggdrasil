// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Yggdrasil.Network.Framing;

namespace Yggdrasil.Network.WebSocket
{
	/// <summary>
	/// Framer for WebSocket messages, as per RFC6455.
	/// </summary>
	/// <remarks>
	/// https://tools.ietf.org/html/rfc6455
	/// </remarks>
	public class WebSocketFramer : IMessageFramer
	{
		public const int MinHeaderLength = 2;

		private byte[] _headerBuffer, _messageBuffer;
		private int _bytesReceived, _headerLength = MinHeaderLength;

		private RNGCryptoServiceProvider _cryptoServiceProvider;

		/// <summary>
		/// Maximum size of messages.
		/// </summary>
		public int MaxMessageSize { get; private set; }

		/// <summary>
		/// Called every time ReceiveData got a full message.
		/// </summary>
		public event Action<byte[]> MessageReceived;

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="maxMessageSize">Maximum size of messages</param>
		public WebSocketFramer(int maxMessageSize)
		{
			this.MaxMessageSize = maxMessageSize;

			// fin+rsv (1), mask+payload_len (1), extended_payload_len (0~8), mask (0~4)
			_headerBuffer = new byte[sizeof(byte) * 2 + sizeof(long) + sizeof(int)];
		}

		/// <summary>
		/// En- or decodes value with the 4 byte mask.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="offsetVal"></param>
		/// <param name="length"></param>
		/// <param name="mask"></param>
		/// <param name="offsetMask"></param>
		/// <returns></returns>
		public static byte[] EnDecode(byte[] value, int offsetVal, int length, byte[] mask, int offsetMask)
		{
			var result = new byte[length];

			for (var i = 0; i < length; ++i)
				result[i] = (byte)(value[offsetVal + i] ^ mask[offsetMask + (i % 4)]);

			return result;
		}

		/// <summary>
		/// Returns a new mask, ready to be used
		/// </summary>
		/// <returns></returns>
		private byte[] GetMask()
		{
			if (_cryptoServiceProvider == null)
				_cryptoServiceProvider = new RNGCryptoServiceProvider();

			var result = new byte[4];
			_cryptoServiceProvider.GetBytes(result);

			return result;
		}

		/// <summary>
		/// En- or decodes value with the 4 byte mask.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="offsetVal"></param>
		/// <param name="length"></param>
		/// <param name="mask"></param>
		/// <param name="offsetMask"></param>
		/// <returns></returns>
		public static void EnDecode(ref byte[] value, int offsetVal, int length, byte[] mask, int offsetMask)
		{
			for (var i = 0; i < length; ++i)
				value[i] = (byte)(value[offsetVal + i] ^ mask[offsetMask + (i % 4)]);
		}

		/// <summary>
		/// Frames given bytes as binary data message without mask.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public byte[] Frame(byte[] message)
		{
			return this.Frame(message, false, FrameOpCode.BinaryData);
		}

		/// <summary>
		/// Frames given bytes as binary data message.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public byte[] Frame(byte[] message, bool useMask)
		{
			return this.Frame(message, useMask, FrameOpCode.BinaryData);
		}

		/// <summary>
		/// Frames given string as UTF8 text message without mask.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public byte[] Frame(string message)
		{
			var bytes = Encoding.UTF8.GetBytes(message);
			return this.Frame(bytes, false, FrameOpCode.UTF8TextData);
		}

		/// <summary>
		/// Frames given string as UTF8 text message.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public byte[] Frame(string message, bool useMask)
		{
			var bytes = Encoding.UTF8.GetBytes(message);
			return this.Frame(bytes, useMask, FrameOpCode.UTF8TextData);
		}

		/// <summary>
		/// Frames given bytes.
		/// </summary>
		/// <param name="message">Payload, accepts null as zero length payload.</param>
		/// <param name="useMask">Whether to mask the payload.</param>
		/// <param name="opCode">What op code is to be set.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentException">
		/// Thrown if given op code can't have a message, but one is given.
		/// </exception>
		public byte[] Frame(byte[] message, bool useMask, FrameOpCode opCode)
		{
			if (message != null && message.Length > 0 && opCode != FrameOpCode.BinaryData && opCode != FrameOpCode.UTF8TextData && opCode != FrameOpCode.Continuation)
				throw new ArgumentException("Only data messages can have a payload.");

			var payloadLength = (message != null ? message.Length : 0);
			var payloadStart = 2;

			// fin, rsv1, rsv2, rsv3, opcode, mask, payload length
			var headerLength = sizeof(byte) * 2;

			if (payloadLength <= 125)
			{
			}
			else if (payloadLength <= short.MaxValue)
			{
				headerLength += sizeof(short); // extended payload length
				payloadStart += sizeof(short);
			}
			else
			{
				headerLength += sizeof(long); // extended payload length 2
				payloadStart += sizeof(long);
			}

			if (useMask)
			{
				headerLength += sizeof(byte) * 4;
				payloadStart += sizeof(byte) * 4;
			}

			var result = new byte[headerLength + payloadLength];

			result[0] |= 0b10000000; // fin, rsv1, rsv2, rsv3
			result[0] |= (byte)((byte)opCode & ~0xF0); // opcode

			if (useMask)
				result[1] |= 0b10000000; // mask

			// payload length
			if (payloadLength <= 125)
			{
				result[1] |= (byte)payloadLength;
			}
			else if (payloadLength <= short.MaxValue)
			{
				result[1] |= 0b01111110;
				var val = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)payloadLength));
				Buffer.BlockCopy(val, 0, result, 2, val.Length);
			}
			else
			{
				result[1] |= 0b01111111;
				var val = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((long)payloadLength));
				Buffer.BlockCopy(val, 0, result, 2, val.Length);
			}

			// mask
			if (useMask)
			{
				var mask = this.GetMask();
				Buffer.BlockCopy(mask, 0, result, payloadStart - sizeof(int), mask.Length);

				if (message != null)
				{
					var maskedPayload = EnDecode(message, 0, message.Length, mask, 0);
					message = maskedPayload;
				}
			}

			// payload
			if (message != null)
				Buffer.BlockCopy(message, 0, result, payloadStart, payloadLength);

			return result;
		}

		/// <summary>
		/// Receives data.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="length"></param>
		public void ReceiveData(byte[] data, int length)
		{
			var bytesAvailable = length;
			if (bytesAvailable == 0)
				return;

			for (var i = 0; i < bytesAvailable;)
			{
				if (_messageBuffer == null)
				{
					var read = Math.Min(_headerLength - _bytesReceived, bytesAvailable - i);
					Buffer.BlockCopy(data, i, _headerBuffer, _bytesReceived, read);

					_bytesReceived += read;
					i += read;

					if (_bytesReceived == _headerLength)
					{
						int lenCode;

						// The length of the header (the data that comes
						// before the payload) changes based on the bits set
						// in the first two byte. If it's not the minimum,
						// we have to wait for more information, to determine
						// how long the entire frame will be.
						if (_headerLength == MinHeaderLength)
						{
							var prevLen = _headerLength;

							if ((_headerBuffer[1] & 0b10000000) != 0)
								_headerLength += sizeof(byte) * 4;

							lenCode = (_headerBuffer[1] & ~0b10000000);
							if (lenCode == 126)
								_headerLength += sizeof(short);
							else if (lenCode == 127)
								_headerLength += sizeof(long);

							if (_headerLength != prevLen)
								continue;
						}

						// Once we know the length of the whole frame,
						// we can start to read it. We'll return the whole
						// frame to the caller, as it contains data that's
						// of interest to them.
						var messageSize = _headerLength;

						lenCode = (_headerBuffer[1] & ~0b10000000);
						if (lenCode <= 125)
							messageSize += lenCode;
						else if (lenCode == 126)
							messageSize += IPAddress.NetworkToHostOrder(BitConverter.ToInt16(_headerBuffer, sizeof(byte) * 2));
						else if (lenCode == 127)
							messageSize += (int)IPAddress.NetworkToHostOrder(BitConverter.ToInt64(_headerBuffer, sizeof(byte) * 2));

						if (messageSize < 0 || messageSize > this.MaxMessageSize)
							throw new InvalidMessageSizeException("Invalid size (" + messageSize + ").");

						_messageBuffer = new byte[messageSize];
						_bytesReceived = _headerLength;

						Buffer.BlockCopy(_headerBuffer, 0, _messageBuffer, 0, _headerLength);
					}
				}

				if (_messageBuffer != null)
				{
					var read = Math.Min(_messageBuffer.Length - _bytesReceived, bytesAvailable - i);
					Buffer.BlockCopy(data, i, _messageBuffer, _bytesReceived, read);

					_bytesReceived += read;
					i += read;

					if (_bytesReceived == _messageBuffer.Length)
					{
						this.MessageReceived?.Invoke(_messageBuffer);

						_messageBuffer = null;
						_bytesReceived = 0;
						_headerLength = MinHeaderLength;
					}
				}
			}
		}
	}

	/// <summary>
	/// OP code for a WebSocket message.
	/// </summary>
	public enum FrameOpCode
	{
		Continuation = 0x00,
		UTF8TextData = 0x01,
		BinaryData = 0x02,
		Close = 0x08,
		Ping = 0x09,
		Pong = 0x0A,
	}
}
