using System;

namespace Yggdrasil.Network.Communication.Messages
{
	/// <summary>
	/// First message sent and received when making a connection.
	/// </summary>
	[Serializable]
	public class HelloMessage : ICommMessage
	{
		/// <summary>
		/// Gets or sets the name of the communicator sending the message.
		/// </summary>
		public string Name { get; set; }
	}
}
