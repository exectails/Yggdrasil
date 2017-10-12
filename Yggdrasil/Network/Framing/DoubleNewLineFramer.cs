// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Text;

namespace Yggdrasil.Network.Framing
{
	/// <summary>
	/// Framer for text messages that end with a double-new-line (CR LF * 2).
	/// </summary>
	public class DoubleNewLineFramer : IMessageFramer
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
		public DoubleNewLineFramer(int maxMessageSize)
		{
			this.MaxMessageSize = maxMessageSize;
			_buffer = new byte[this.MaxMessageSize];
		}

		/// <summary>
		/// Appends terminating double-new-line to message.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public byte[] Frame(byte[] message)
		{
			var messageLength = message.Length;
			if (messageLength > this.MaxMessageSize)
				throw new InvalidMessageSizeException("Invalid size (" + messageLength + ").");

			var result = new byte[message.Length + 4];
			Buffer.BlockCopy(message, 0, result, 0, message.Length);
			result[result.Length - 4] = (byte)'\r';
			result[result.Length - 3] = (byte)'\n';
			result[result.Length - 2] = (byte)'\r';
			result[result.Length - 1] = (byte)'\n';

			return result;
		}

		/// <summary>
		/// Appends terminating double-new-line to message.
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

			for (var i = 0; i < bytesAvailable; ++i)
			{
				_buffer[_bytesReceived] = data[i];
				_bytesReceived++;

				if (_bytesReceived > this.MaxMessageSize)
					throw new InvalidMessageSizeException("Invalid size (" + _bytesReceived + ").");

				var end = (_bytesReceived >= 4 && _buffer[_bytesReceived - 4] == (byte)'\r' && _buffer[_bytesReceived - 3] == (byte)'\n' && _buffer[_bytesReceived - 2] == (byte)'\r' && _buffer[_bytesReceived - 1] == (byte)'\n');

				if (end)
				{
					var message = Encoding.UTF8.GetString(_buffer, 0, _bytesReceived - 4);
					this.MessageReceived?.Invoke(message);
					_bytesReceived = 0;
				}
			}
		}
	}
}
