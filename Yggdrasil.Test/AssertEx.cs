using System;
using Xunit;

namespace Yggdrasil.Test
{
	internal class AssertEx : Xunit.Assert
	{
		/// <summary>
		/// Verifies that a block of code does not throw any exceptions.
		/// </summary>
		/// <param name="testCode">A delegate to the code to be tested</param>
		/// <exception cref="DoesNotThrowException">Thrown when the code throws an exception.</exception>
		public static void DoesNotThrow(Action testCode)
		{
			var ex = Record.Exception(testCode);

			if (ex != null)
				throw new DoesNotThrowException(ex);
		}
	}

	/// <summary>
	/// Exception thrown when code unexpectedly throws an exception.
	/// </summary>
	/// <param name="actual">Actual exception</param>
	internal class DoesNotThrowException(Exception actual) : Exception("Delegate threw an exception.", actual)
	{
	}
}
