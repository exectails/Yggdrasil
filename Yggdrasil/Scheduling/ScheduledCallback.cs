using System;

namespace Yggdrasil.Scheduling
{
	/// <summary>
	/// A callback that is scheduled to be exected and potentially repeated
	/// after certain delays.
	/// </summary>
	public class ScheduledCallback : IComparable<ScheduledCallback>, IComparable
	{
		/// <summary>
		/// Get sor sets the delay until the callback is executed.
		/// </summary>
		public TimeSpan Delay { get; set; }

		/// <summary>
		/// Returns the delay before the callback is executed again.
		/// </summary>
		public TimeSpan RepeatDelay { get; }

		/// <summary>
		/// Returns the callback that is executed after the delays.
		/// </summary>
		public Action Callback { get; }

		/// <summary>
		/// Creates new callback.
		/// </summary>
		/// <param name="delay"></param>
		/// <param name="repeatDelay"></param>
		/// <param name="callback"></param>
		public ScheduledCallback(TimeSpan delay, TimeSpan repeatDelay, Action callback)
		{
			this.Delay = delay;
			this.RepeatDelay = repeatDelay;
			this.Callback = callback;
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

}
