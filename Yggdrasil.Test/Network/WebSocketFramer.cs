// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Yggdrasil.Network.WebSocket;

namespace Yggdrasil.Test.Network
{
	public class WebSocketFramerTests
	{
		[Fact]
		public void EnDecode()
		{
			var val = new byte[] { 1, 2, 3 };
			var mask = new byte[] { 0x0A, 0x0B, 0x0C, 0x0D };
			var encoded = WebSocketFramer.EnDecode(val, 0, val.Length, mask, 0);
			var decoded = WebSocketFramer.EnDecode(encoded, 0, val.Length, mask, 0);
			Assert.Equal(new byte[] { 1 ^ 0x0A, 2 ^ 0x0B, 3 ^ 0x0C }, encoded);
			Assert.Equal(new byte[] { 1, 2, 3 }, decoded);

			val = new byte[] { 1, 2, 3, 4, 5, 6 };
			mask = new byte[] { 0x0A, 0x0B, 0x0C, 0x0D };
			encoded = WebSocketFramer.EnDecode(val, 0, val.Length, mask, 0);
			decoded = WebSocketFramer.EnDecode(encoded, 0, val.Length, mask, 0);
			Assert.Equal(new byte[] { 1 ^ 0x0A, 2 ^ 0x0B, 3 ^ 0x0C, 4 ^ 0x0D, 5 ^ 0x0A, 6 ^ 0x0B }, encoded);
			Assert.Equal(new byte[] { 1, 2, 3, 4, 5, 6 }, decoded);

			var combined = new byte[] { 0, 0, 0, 1, 2, 3, 0x0A, 0x0B, 0x0C, 0x0D, 0, 0, 0 };
			encoded = WebSocketFramer.EnDecode(combined, 3, 3, combined, 6);
			decoded = WebSocketFramer.EnDecode(encoded, 0, 3, combined, 6);
			Assert.Equal(new byte[] { 1 ^ 0x0A, 2 ^ 0x0B, 3 ^ 0x0C }, encoded);
			Assert.Equal(new byte[] { 1, 2, 3 }, decoded);
		}

		[Fact]
		public void FrameUnmasked()
		{
			var framer = new WebSocketFramer(1024);

			var buffer = framer.Frame(null, false, FrameOpCode.Close);
			Assert.Equal(new byte[] { 0x88, 0x00 }, buffer);

			buffer = framer.Frame("123", false);
			Assert.Equal(new byte[] { 0x81, 0x03, 0x31, 0x32, 0x33 }, buffer);

			buffer = framer.Frame(new byte[] { 1, 2, 3 }, false);
			Assert.Equal(new byte[] { 0x82, 0x03, 0x01, 0x02, 0x03 }, buffer);

			buffer = framer.Frame(new byte[125], false);
			var expected = new byte[2 + 125];
			expected[0] = 0x82;
			expected[1] = 0x7D;
			Assert.Equal(expected, buffer);

			buffer = framer.Frame(new byte[126], false);
			expected = new byte[2 + 2 + 126];
			expected[0] = 0x82;
			expected[1] = 0x7E;
			expected[2] = 0x00;
			expected[3] = 0x7E;
			Assert.Equal(expected, buffer);

			buffer = framer.Frame(new byte[short.MaxValue + 10], false);
			expected = new byte[2 + 8 + short.MaxValue + 10];
			expected[0] = 0x82;
			expected[1] = 0x7F;
			expected[2] = 0x00;
			expected[3] = 0x00;
			expected[4] = 0x00;
			expected[5] = 0x00;
			expected[6] = 0x00;
			expected[7] = 0x00;
			expected[8] = 0x80;
			expected[9] = 0x09;
			Assert.Equal(expected, buffer);

			Assert.Throws<ArgumentException>(() => framer.Frame(new byte[1], false, FrameOpCode.Close));
			Assert.Throws<ArgumentException>(() => framer.Frame(new byte[1], false, FrameOpCode.Ping));
			Assert.Throws<ArgumentException>(() => framer.Frame(new byte[1], false, FrameOpCode.Pong));
			Assert.DoesNotThrow(() => framer.Frame(new byte[1], false, FrameOpCode.BinaryData));
			Assert.DoesNotThrow(() => framer.Frame(new byte[1], false, FrameOpCode.TextData));
			Assert.DoesNotThrow(() => framer.Frame(new byte[1], false, FrameOpCode.Continuation));
		}

		[Fact]
		public void FrameMasked()
		{
			var framer = new WebSocketFramer(1024);

			var val = new byte[] { 1, 2, 3 };
			var buffer = framer.Frame(val, true);
			var headerLength = 6;
			Assert.Equal(headerLength + val.Length, buffer.Length);
			Assert.Equal(0x82, buffer[0]);
			Assert.Equal(0x83, buffer[1]);
			Console.WriteLine(BitConverter.ToString(buffer));
			Console.WriteLine(BitConverter.ToString(buffer, headerLength, val.Length));
			Console.WriteLine(BitConverter.ToString(buffer, headerLength - 4, 4));
			Assert.Equal(new byte[] { 1, 2, 3 }, WebSocketFramer.EnDecode(buffer, headerLength, val.Length, buffer, headerLength - 4));
		}

		[Fact]
		public void ReceiveDataUnmasked_Single()
		{
			var framer = new WebSocketFramer(1024);
			var receivedMessages = new List<byte[]>();

			framer.MessageReceived += buffer => { receivedMessages.Add(buffer); };

			var data = framer.Frame(new byte[] { 1, 2, 3 }, false);
			framer.ReceiveData(data, data.Length);

			Assert.Equal(1, receivedMessages.Count);
			Assert.Equal(data, receivedMessages[0]);
		}

		[Fact]
		public void ReceiveDataMasked_Single()
		{
			var framer = new WebSocketFramer(1024);
			var receivedMessages = new List<byte[]>();

			framer.MessageReceived += buffer => { receivedMessages.Add(buffer); };

			var data = framer.Frame(new byte[] { 1, 2, 3 }, true);
			framer.ReceiveData(data, data.Length);

			Assert.Equal(1, receivedMessages.Count);
			Assert.Equal(data, receivedMessages[0]);
		}

		[Fact]
		public void ReceiveDataMaskedShort_Single()
		{
			Console.WriteLine("--- ReceiveDataSingleMaskedShort");
			var framer = new WebSocketFramer(102400);
			var receivedMessages = new List<byte[]>();

			framer.MessageReceived += buffer => { receivedMessages.Add(buffer); };

			var data = framer.Frame(new byte[512], true);
			framer.ReceiveData(data, data.Length);

			Assert.Equal(1, receivedMessages.Count);
			Assert.Equal(data, receivedMessages[0]);
			Console.WriteLine("---");
		}

		[Fact]
		public void ReceiveDataMaskedLong_Single()
		{
			var framer = new WebSocketFramer(102400);
			var receivedMessages = new List<byte[]>();

			framer.MessageReceived += buffer => { receivedMessages.Add(buffer); };

			var data = framer.Frame(new byte[short.MaxValue + 10], true);
			framer.ReceiveData(data, data.Length);

			Assert.Equal(1, receivedMessages.Count);
			Assert.Equal(data, receivedMessages[0]);
		}

		[Fact]
		public void ReceiveData_Fragmented()
		{
			var framer = new WebSocketFramer(1024);
			var receivedMessages = new List<byte[]>();

			framer.MessageReceived += buffer => { receivedMessages.Add(buffer); };

			// Check receiving a different amount of bytes each time
			foreach (var steps in new int[] { 1, 2, 3, 4, 5 })
			{
				// Create messages
				var message1 = new byte[] { 1, 2, 3, 4, 5 };
				var message2 = new byte[] { 5, 6, 7 };
				var message3 = Encoding.UTF8.GetBytes("foobar");
				var message4 = new byte[512];

				var framed1 = framer.Frame(message1, false);
				var framed2 = framer.Frame(message2, true);
				var framed3 = framer.Frame(message3, false);
				var framed4 = framer.Frame(message4, true);

				// Put messages into one array
				var dataList = new List<byte>();
				dataList.AddRange(framed1);
				dataList.AddRange(framed2);
				dataList.AddRange(framed3);
				dataList.AddRange(framed4);

				// Receive data step by step
				for (var i = 0; i < dataList.Count; i += steps)
				{
					var take = Math.Min(dataList.Count - i, steps);
					if (take < 0)
						continue;

					var data = dataList.Skip(i).Take(take).ToArray();
					framer.ReceiveData(data, data.Length);
				}

				// Check
				Assert.Equal(4, receivedMessages.Count);
				Assert.Equal(framed1, receivedMessages[0]);
				Assert.Equal(framed2, receivedMessages[1]);
				Assert.Equal(framed3, receivedMessages[2]);
				Assert.Equal(framed4, receivedMessages[3]);
				receivedMessages.Clear();
			}
		}
	}
}
