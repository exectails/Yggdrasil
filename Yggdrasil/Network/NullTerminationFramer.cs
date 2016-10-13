// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yggdrasil.Network
{
	public class NullTerminationFramer : IMessageFramer
	{
		private byte[] _buffer;
		private int _bytesReceived;

		/// <summary>
		/// Maximum size of messages.
		/// </summary>
		public int MaxMessageSize { get; private set; }

		/// <summary>
		/// Called every time ReceiveData got a full message.
		/// </summary>
		public event Action<string> MessageReceived;

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="maxMessageSize">Maximum size of messages</param>
		public NullTerminationFramer(int maxMessageSize)
		{
			this.MaxMessageSize = maxMessageSize;
			_buffer = new byte[this.MaxMessageSize];
		}

		/// <summary>
		/// Appends terminating null-byte to message.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public byte[] Frame(byte[] message)
		{
			var messageLength = message.Length;
			if (messageLength > this.MaxMessageSize)
				throw new InvalidMessageSizeException("Invalid size (" + messageLength + ").");

			var result = new byte[message.Length + 1];
			Buffer.BlockCopy(message, 0, result, 0, message.Length);

			return result;
		}

		/// <summary>
		/// Appends terminating null-char to message.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public byte[] Frame(string message)
		{
			return this.Frame(Encoding.UTF8.GetBytes(message));
		}

		/// <summary>
		/// Receives data and calls MessageReceived every time a full message
		/// has arrived.
		/// </summary>
		/// <param name="data">Buffer to read from.</param>
		/// <param name="length">Length of actual information in data.</param>
		/// <exception cref="InvalidMessageSizeException">
		/// Thrown if a message has an invalid size. Should this occur,
		/// the connection should be terminated, because it's not save to
		/// keep receiving anymore.
		/// </exception>
		public void ReceiveData(byte[] data, int length)
		{
			var bytesAvailable = length;
			if (bytesAvailable == 0)
				return;

			var start = 0;

			for (int i = 0; i <= bytesAvailable; ++i)
			{
				var isEnd = (i == bytesAvailable);
				var isZero = !isEnd && (data[i] == 0);

				if (isZero || isEnd)
				{
					var read = i - start;
					if (_bytesReceived + read > this.MaxMessageSize)
						throw new InvalidMessageSizeException("Invalid size (" + (_bytesReceived + read) + ").");

					Buffer.BlockCopy(data, start, _buffer, _bytesReceived, read);

					_bytesReceived += read;

					if (isZero)
					{
						var ev = this.MessageReceived;
						if (ev != null)
							ev(Encoding.UTF8.GetString(_buffer, 0, _bytesReceived));

						_bytesReceived = 0;
						start = i + 1;
					}
				}
			}
		}
	}
}
