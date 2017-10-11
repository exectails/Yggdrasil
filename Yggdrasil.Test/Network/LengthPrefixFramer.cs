// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Yggdrasil.Network;
using Yggdrasil.Network.Framing;

namespace Yggdrasil.Test.Network
{
	public class LengthPrefixFramerTests
	{
		[Fact]
		public void Frame()
		{
			var maxMessageSize = 10;
			var framer = new LengthPrefixFramer(maxMessageSize);

			// Check result
			var message = new byte[] { 1, 2, 3, 4, 5 };
			Assert.Equal(new byte[] { 9, 0, 0, 0, 1, 2, 3, 4, 5 }, framer.Frame(message));

			// Check max size exception
			var invalidMessage = new byte[maxMessageSize + 1];
			Assert.Throws<InvalidMessageSizeException>(() => framer.Frame(invalidMessage));
		}

		[Fact]
		public void ReceiveData_Single()
		{
			byte[] receivedMessage = null;

			var maxMessageSize = 10;
			var framer = new LengthPrefixFramer(maxMessageSize);
			framer.MessageReceived += (msg => receivedMessage = msg);

			var message = new byte[] { 1, 2, 3, 4, 5 };
			var data = framer.Frame(message);
			framer.ReceiveData(data, data.Length);

			Assert.Equal(message, receivedMessage);
		}

		[Fact]
		public void ReceiveData_Multiple()
		{
			var receivedMessages = new List<byte[]>();

			var maxMessageSize = 10;
			var framer = new LengthPrefixFramer(maxMessageSize);
			framer.MessageReceived += (msg => receivedMessages.Add(msg));

			// Create messages
			var message1 = new byte[] { 1, 2, 3, 4, 5 };
			var message2 = new byte[] { 5, 6, 7 };
			var message3 = Encoding.UTF8.GetBytes("foobar");

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
			var receivedMessages = new List<byte[]>();

			var maxMessageSize = 10;
			var framer = new LengthPrefixFramer(maxMessageSize);
			framer.MessageReceived += (msg => receivedMessages.Add(msg));

			// Check receiving a different amount of bytes each time
			foreach (var steps in new int[] { 1, 2, 3, 4, 5 })
			{
				// Create messages
				var message1 = new byte[] { 1, 2, 3, 4, 5 };
				var message2 = new byte[] { 5, 6, 7 };
				var message3 = Encoding.UTF8.GetBytes("foobar");

				// Put messages into one array
				var dataList = new List<byte>();
				dataList.AddRange(framer.Frame(message1));
				dataList.AddRange(framer.Frame(message2));
				dataList.AddRange(framer.Frame(message3));

				// Receive data step by step
				for (int i = 0; i < dataList.Count; i += steps)
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

		[Fact]
		public void ReceiveData_FragmentedInvalid()
		{
			var receivedMessages = new List<byte[]>();

			var maxMessageSize = 10;
			var framer = new LengthPrefixFramer(maxMessageSize);
			framer.MessageReceived += (msg => receivedMessages.Add(msg));

			// Create messages
			var message1 = new byte[] { 1, 2, 3, 4, 5 };
			var message2 = new byte[] { 5, 6, 7 };
			var message3 = Encoding.UTF8.GetBytes("foobar too long");
			var message4 = Encoding.UTF8.GetBytes("foobar");

			// Put messages into one array
			var dataList = new List<byte>();
			dataList.AddRange(framer.Frame(message1));
			dataList.AddRange(framer.Frame(message2));
			dataList.AddRange(new byte[] { 19, 0, 0, 0 });
			dataList.AddRange(message3);
			dataList.AddRange(framer.Frame(message4));

			// Receive data step by step
			var steps = 3;
			for (int i = 0; i < dataList.Count; i += steps)
			{
				var take = Math.Min(dataList.Count - i, steps);
				if (take < 0)
					continue;

				var data = dataList.Skip(i).Take(take).ToArray();
				if (i < 18)
				{
					framer.ReceiveData(data, data.Length);
				}
				else
				{
					Assert.Throws<InvalidMessageSizeException>(() => framer.ReceiveData(data, data.Length));
					break;
				}
			}

			// Check
			// After the third message failed due an exception for being
			// too long, we should've received only the first two.
			Assert.Equal(2, receivedMessages.Count);
			Assert.Equal(message1, receivedMessages[0]);
			Assert.Equal(message2, receivedMessages[1]);
		}
	}
}
