using System;
using Yggdrasil.Network.Framing;
using Yggdrasil.Network.TCP;

namespace Yggdrasil.Network.Communication
{
	/// <summary>
	/// A connection from a client to a communicator.
	/// </summary>
	public class Connection : TcpConnection
	{
		private const int MaxMessageSize = 1024 * 50;

		private readonly LengthPrefixFramer _framer;

		/// <summary>
		/// Gets or sets the name of the connected communicator.
		/// </summary>
		public string Name { get; internal set; }

		/// <summary>
		/// Called when a message was received.
		/// </summary>
		public event Action<Connection, byte[]> MessageReceived;

		/// <summary>
		/// Creates new instance.
		/// </summary>
		public Connection()
		{
			_framer = new LengthPrefixFramer(MaxMessageSize);
			_framer.MessageReceived += this.OnMessageReceived;
		}

		/// <summary>
		/// Called when the client sent data.
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="length"></param>
		protected override void ReceiveData(byte[] buffer, int length)
			=> _framer.ReceiveData(buffer, length);

		/// <summary>
		/// Called when a full message was received.
		/// </summary>
		/// <param name="buffer"></param>
		private void OnMessageReceived(byte[] buffer)
			=> this.MessageReceived?.Invoke(this, buffer);

		/// <summary>
		/// Sends object to the connected communicator.
		/// </summary>
		/// <param name="buffer"></param>
		public override void Send(byte[] buffer)
		{
			buffer = _framer.Frame(buffer);
			base.Send(buffer);
		}
	}
}
