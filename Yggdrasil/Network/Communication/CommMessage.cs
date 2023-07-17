namespace Yggdrasil.Network.Communication
{
	/// <summary>
	/// A message sent via a communicator.
	/// </summary>
	public interface ICommMessage
	{
	}

	/// <summary>
	/// Extensions for communicator messages.
	/// </summary>
	public static class ICommMessageExtensions
	{
		/// <summary>
		/// Wraps the message in a broadcast message.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="channelName"></param>
		/// <returns></returns>
		public static ICommMessage BroadcastTo(this ICommMessage message, string channelName)
			=> new Messages.BroadcastMessage(channelName, message);
	}
}
