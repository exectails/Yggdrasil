using System;
using System.Text.Json.Serialization;
using System.Threading;

namespace Yggdrasil.Network.Communication.Messages
{
	/// <summary>
	/// A wrapper around a message that is sent to request a response.
	/// </summary>
	[Serializable]
	public class RequestMessage : ICommMessage
	{
		private static long Ids = 0;

		/// <summary>
		/// Returns the unique id of the request.
		/// </summary>
		public long Id { get; } = Interlocked.Increment(ref Ids);

		/// <summary>
		/// Returns the message that a response is requested for.
		/// </summary>
		[JsonConverter(typeof(InterfaceConverter<ICommMessage>))]
		public ICommMessage Message { get; }

		/// <summary>
		/// Creates a new request message for the given message.
		/// </summary>
		/// <param name="message"></param>
		public RequestMessage(ICommMessage message)
		{
			this.Message = message;
		}
	}
}
