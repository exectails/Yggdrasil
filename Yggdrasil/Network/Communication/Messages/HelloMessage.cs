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
		/// Gets or sets the authentication for the communication.
		/// </summary>
		/// <remarks>
		/// Connection requests will be rejected if this value is not correct.
		/// </remarks>
		public string Authentication { get; set; }

		/// <summary>
		/// Creates new message.
		/// </summary>
		/// <param name="name">Name of the connecting communicator.</param>
		/// <param name="authentication">Authentication value for the other side to validate the connection.</param>
		public HelloMessage(string name, string authentication)
		{
			this.Name = name;
			this.Authentication = authentication;
		}
	}
}
