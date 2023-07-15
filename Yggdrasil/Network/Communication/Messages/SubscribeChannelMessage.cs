using System;

namespace Yggdrasil.Network.Communication.Messages
{
	/// <summary>
	/// Subscribes sender to message broadcasted on a channel.
	/// </summary>
	[Serializable]
	public class SubscribeChannelMessage : ICommMessage
	{
		/// <summary>
		/// Channel to subscribe to.
		/// </summary>
		public string ChannelName { get; }

		/// <summary>
		/// Creates new message.
		/// </summary>
		/// <param name="channelName"></param>
		public SubscribeChannelMessage(string channelName)
		{
			this.ChannelName = channelName;
		}
	}
}
