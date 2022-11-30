using System;

namespace Yggdrasil.Scheduling
{
	/// <summary>
	/// A callback that is scheduled to be executed and potentially repeated
	/// after certain delays.
	/// </summary>
	public class ScheduledCallback : IComparable<ScheduledCallback>, IComparable
	{
		/// <summary>
		/// Gets or sets the schedule's id.
		/// </summary>
		public long Id { get; set; }

		/// <summary>
		/// Gets or sets the delay until the callback is executed.
		/// </summary>
		public TimeSpan Delay { get; set; }

		/// <summary>
		/// Returns the delay before the callback is executed again.
		/// </summary>
		public TimeSpan RepeatDelay { get; }

		/// <summary>
		/// Returns the time since the callback was scheduled or last
		/// executed.
		/// </summary>
		public TimeSpan Elapsed { get; internal set; }

		/// <summary>
		/// Returns how many times the callback was executed.
		/// </summary>
		public long ExecuteCount { get; internal set; }

		/// <summary>
		/// Returns the callback that is executed after the delays.
		/// </summary>
		public ScheduledCallbackFunc Callback { get; }

		/// <summary>
		/// Returns a list of arguments that are passed to the callback
		/// when it's executed. The return value may be null if no para-
		/// meters were set.
		/// </summary>
		public object[] Arguments { get; }

		/// <summary>
		/// Creates new callback.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="delay"></param>
		/// <param name="repeatDelay"></param>
		/// <param name="callback"></param>
		/// <param name="arguments"></param>
		public ScheduledCallback(long id, TimeSpan delay, TimeSpan repeatDelay, ScheduledCallbackFunc callback, object[] arguments)
		{
			this.Id = id;
			this.Delay = delay;
			this.RepeatDelay = repeatDelay;
			this.Callback = callback;
			this.Arguments = arguments;
		}

		/// <summary>
		/// Compares the two scheduled callbacks based on their delay.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public int CompareTo(ScheduledCallback other)
		{
			return this.Delay.CompareTo(other.Delay);
		}

		/// <summary>
		/// Caompares this scheduler with the given object.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public int CompareTo(object obj)
		{
			if (obj is null || !(obj is ScheduledCallback other))
				return -1;

			return this.CompareTo(other);
		}
	}

	/// <summary>
	/// A callback function for a scheduled execution.
	/// </summary>
	/// <param name="result"></param>
	public delegate void ScheduledCallbackFunc(CallbackState result);
}
