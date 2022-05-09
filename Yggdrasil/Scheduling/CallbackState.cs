using System;

namespace Yggdrasil.Scheduling
{
	/// <summary>
	/// Passed to scheduled callbacks.
	/// </summary>
	public struct CallbackState
	{
		/// <summary>
		/// The time that has passed since the callback was scheduled or
		/// last executed.
		/// </summary>
		public readonly TimeSpan Elapsed;

		/// <summary>
		/// How many times the callback was executed.
		/// </summary>
		public readonly long ExecuteCount;

		/// <summary>
		/// The arguments that were set for this callback.
		/// </summary>
		public readonly object[] Arguments;

		/// <summary>
		/// Creates new result.
		/// </summary>
		/// <param name="elapsed"></param>
		/// <param name="executeCount"></param>
		/// <param name="arguments"></param>
		internal CallbackState(TimeSpan elapsed, long executeCount, object[] arguments)
		{
			this.Elapsed = elapsed;
			this.ExecuteCount = executeCount;
			this.Arguments = arguments;
		}
	}
}
