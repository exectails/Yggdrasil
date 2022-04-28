using System;
using Yggdrasil.Network.Framing;
using Yggdrasil.Network.TCP;

namespace Yggdrasil.Network.Communication
{
	/// <summary>
	/// A client to connect to communicators.
	/// </summary>
	public class Client : TcpClient
	{
		private const int MaxMessageSize = 1024 * 50;

		private readonly LengthPrefixFramer _framer;

		/// <summary>
		/// Returns the remote communicator's name.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Called when a message was received.
		/// </summary>
		public event Action<Client, byte[]> MessageReceived;

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="name"></param>
		public Client(string name)
		{
			_framer = new LengthPrefixFramer(MaxMessageSize);
			_framer.MessageReceived += this.OnMessageReceived;

			this.Name = name;
		}

		/// <summary>
		/// Called when the client received data.
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
