using System;

namespace Yggdrasil.Network.Communication.Messages
{
	/// <summary>
	/// A wrapper around a message that is sent as a response to a request.
	/// </summary>
	[Serializable]
	public class ResponseMessage : ICommMessage
	{
		/// <summary>
		/// Returns the unique id of the request.
		/// </summary>
		public long Id { get; }

		/// <summary>
		/// Returns the actual response message.
		/// </summary>
		public ICommMessage Message { get; }

		/// <summary>
		/// Creates a new response message for the given id and message.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="message"></param>
		public ResponseMessage(long id, ICommMessage message)
		{
			this.Id = id;
			this.Message = message;
		}
	}
}
