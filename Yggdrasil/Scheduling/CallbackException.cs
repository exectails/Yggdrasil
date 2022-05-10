using System;

namespace Yggdrasil.Scheduling
{
	/// <summary>
	/// An exception that happened while executing a scheduled callback.
	/// </summary>
	public class CallbackException : Exception
	{
		/// <summary>
		/// Creates new exception.
		/// </summary>
		/// <param name="innerException"></param>
		public CallbackException(Exception innerException) : base(innerException.Message, innerException)
		{
		}
	}
}
