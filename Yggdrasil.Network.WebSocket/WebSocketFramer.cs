// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using System;
using System.Net;
using System.Text;
using Yggdrasil.Network.Framing;

namespace Yggdrasil.Network.WebSocket
{
	/// <summary>
	/// Framer for WebSocket messages.
	/// </summary>
	public class WebSocketFramer : IMessageFramer
	{
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
				var mask = BitConverter.GetBytes(0x0A0B0C0D);
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
			throw new NotImplementedException();
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
		TerminateConnection = 0x08,
		Ping = 0x09,
		Pong = 0x0A,
	}
}
