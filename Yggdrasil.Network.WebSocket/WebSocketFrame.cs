// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using System;
using System.Net;

namespace Yggdrasil.Network.WebSocket
{
	/// <summary>
	/// Represents a frame received via WebSocket.
	/// </summary>
	public class WebSocketFrame
	{
		/// <summary>
		/// Specifies whether this frame was the finale one for the
		/// message.
		/// </summary>
		public bool Fin { get; }

		/// <summary>
		/// Returns the frames op-code.
		/// </summary>
		public FrameOpCode OpCode { get; }

		/// <summary>
		/// Returns the frame's data.
		/// </summary>
		public byte[] PayLoad { get; }

		/// <summary>
		/// Creates a new instance, based on buffer.
		/// </summary>
		/// <param name="buffer"></param>
		public WebSocketFrame(byte[] buffer)
		{
			this.Fin = (buffer[0] & 0b10000000) != 0;
			this.OpCode = (FrameOpCode)(buffer[0] & ~0b11110000);
			this.PayLoad = ParsePayload(buffer);
		}

		/// <summary>
		/// Reads the payload from the buffer and returns it.
		/// </summary>
		/// <param name="buffer"></param>
		/// <returns></returns>
		private static byte[] ParsePayload(byte[] buffer)
		{
			var payloadStart = sizeof(byte) * 2;
			var payloadLength = 0;

			var usesMask = (buffer[1] & 0b10000000) != 0;
			if (usesMask)
				payloadStart += sizeof(byte) * 4;

			var lenCode = (buffer[1] & ~0b10000000);
			if (lenCode <= 125)
			{
				payloadStart += 0;
				payloadLength = lenCode;
			}
			else if (lenCode == 126)
			{
				payloadStart += sizeof(short);
				payloadLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(buffer, sizeof(byte) * 2));
			}
			else if (lenCode == 127)
			{
				payloadStart += sizeof(long);
				payloadLength = (int)IPAddress.NetworkToHostOrder(BitConverter.ToInt64(buffer, sizeof(byte) * 2));
			}

			var result = new byte[payloadLength];
			Buffer.BlockCopy(buffer, payloadStart, result, 0, payloadLength);

			if (usesMask)
			{
				var maskStart = payloadStart - sizeof(int);
				WebSocketFramer.EnDecode(ref result, 0, payloadLength, buffer, maskStart);
			}

			return result;
		}
	}
}
