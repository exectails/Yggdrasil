using System;

namespace Yggdrasil.Variables
{
	/// <summary>
	/// Exception thrown when trying to get a variable of a different
	/// type than the one requested.
	/// </summary>
	public class TypeMismatchException : Exception
	{
		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="message"></param>
		public TypeMismatchException(string message) : base(message)
		{
		}
	}
}
