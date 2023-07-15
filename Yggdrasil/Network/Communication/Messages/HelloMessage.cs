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
		/// Returns the name of the communicator sending the message.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Creates new message.
		/// </summary>
		/// <param name="name"></param>
		public HelloMessage(string name)
		{
			this.Name = name;
		}
	}
}
