using System;
using System.Text.Json.Serialization;

namespace Yggdrasil.Network.Communication.Messages
{
	/// <summary>
	/// A message that is broadcasted to all subscribers of a channel.
	/// </summary>
	[Serializable]
	public class BroadcastMessage : ICommMessage
	{
		/// <summary>
		/// Returns the name of the channel to broadcast on.
		/// </summary>
		public string ChannelName { get; }

		/// <summary>
		/// Returns the message to be broadcasted.
		/// </summary>
		[JsonConverter(typeof(InterfaceConverter<ICommMessage>))]
		public ICommMessage Message { get; }

		/// <summary>
		/// Creates new broadcast message.
		/// </summary>
		/// <param name="channelName"></param>
		/// <param name="message"></param>
		public BroadcastMessage(string channelName, ICommMessage message)
		{
			this.ChannelName = channelName;
			this.Message = message;
		}
	}
}
