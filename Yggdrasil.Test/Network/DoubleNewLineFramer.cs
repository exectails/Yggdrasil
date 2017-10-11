// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Yggdrasil.Network.Framing;

namespace Yggdrasil.Test.Network
{
	public class DoubleNewLineFramerTests
	{
		[Fact]
		public void Frame()
		{
			var maxMessageSize = 10;
			var framer = new DoubleNewLineFramer(maxMessageSize);

			// Check result
			var message = new byte[] { 1, 2, 3, 4, 5 };
			Assert.Equal(new byte[] { 1, 2, 3, 4, 5, (byte)'\r', (byte)'\n', (byte)'\r', (byte)'\n' }, framer.Frame(message));

			var messageStr = "12345";
			Assert.Equal(Encoding.UTF8.GetBytes("12345\r\n\r\n"), framer.Frame(messageStr));

			// Check max size exception
			var invalidMessage = new byte[maxMessageSize + 1];
			Assert.Throws<InvalidMessageSizeException>(() => framer.Frame(invalidMessage));
		}

		[Fact]
		public void ReceiveData_Single()
		{
			var receivedMessages = new List<string>();

			var maxMessageSize = 10;
			var framer = new DoubleNewLineFramer(maxMessageSize);
			framer.MessageReceived += (msg => receivedMessages.Add(msg));

			var message = "12345";
			var data = framer.Frame(message);
			framer.ReceiveData(data, data.Length);

			Assert.Equal(1, receivedMessages.Count);
			Assert.Equal(message, receivedMessages[0]);
		}

		[Fact]
		public void ReceiveData_Multiple()
		{
			var receivedMessages = new List<string>();

			var maxMessageSize = 10;
			var framer = new DoubleNewLineFramer(maxMessageSize);
			framer.MessageReceived += (msg => receivedMessages.Add(msg));

			// Create messages
			var message1 = "12345";
			var message2 = "567";
			var message3 = "foobar";

			// Put messages into one array
			var dataList = new List<byte>();
			dataList.AddRange(framer.Frame(message1));
			dataList.AddRange(framer.Frame(message2));
			dataList.AddRange(framer.Frame(message3));

			// Receive the array
			var data = dataList.ToArray();
			framer.ReceiveData(data, data.Length);

			// Check
			Assert.Equal(3, receivedMessages.Count);
			Assert.Equal(message1, receivedMessages[0]);
			Assert.Equal(message2, receivedMessages[1]);
			Assert.Equal(message3, receivedMessages[2]);
		}

		[Fact]
		public void ReceiveData_Fragmented()
		{
			var receivedMessages = new List<string>();

			var maxMessageSize = 10;
			var framer = new DoubleNewLineFramer(maxMessageSize);
			framer.MessageReceived += (msg => receivedMessages.Add(msg));

			// Check receiving a different amount of bytes each time
			foreach (var steps in new int[] { 1, 2, 3, 4, 5 })
			{
				// Create messages
				var message1 = "12345";
				var message2 = "567";
				var message3 = "foobar";

				// Put messages into one array
				var dataList = new List<byte>();
				dataList.AddRange(framer.Frame(message1));
				dataList.AddRange(framer.Frame(message2));
				dataList.AddRange(framer.Frame(message3));

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
				Assert.Equal(3, receivedMessages.Count);
				Assert.Equal(message1, receivedMessages[0]);
				Assert.Equal(message2, receivedMessages[1]);
				Assert.Equal(message3, receivedMessages[2]);
				receivedMessages.Clear();
			}
		}
	}
}
