// https://stackoverflow.com/a/55676794/1171898
// License: public domain (no restrictions or obligations)
// Author: Vitaly Vinogradov

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Yggdrasil.Scheduling
{
	/// <summary>
	/// High resolution scheduler. 
	/// </summary>
	public class Scheduler : IDisposable
	{
		private readonly static TimeSpan HotLoopThreshold = TimeSpan.FromMilliseconds(50);

		private readonly Thread _loopThread;

		private readonly ManualResetEvent _allowed = new ManualResetEvent(false);
		private readonly AutoResetEvent _wakeFromColdLoop = new AutoResetEvent(false);

		private readonly List<ScheduledCallback> _scheduled = new List<ScheduledCallback>();
		private readonly List<ScheduledCallback> _pending = new List<ScheduledCallback>();
		private readonly List<ScheduledCallback> _execute = new List<ScheduledCallback>();

		private bool _newElements;
		private bool _disposing;

		/// <summary>
		/// Returns true if the scheduler is actively handling timers.
		/// </summary>
		public bool IsActive => _allowed.WaitOne(0);

		/// <summary>
		/// Creates new scheduler.
		/// </summary>
		public Scheduler()
		{
			_loopThread = new Thread(this.Loop);
			_loopThread.Start();
		}

		/// <summary>
		/// Starts handling of timers.
		/// </summary>
		public void Start()
		{
			_allowed.Set();
		}

		/// <summary>
		/// Pauses handling of timers.
		/// </summary>
		public void Pause()
		{
			_allowed.Reset();
		}

		/// <summary>
		/// Schedules callback to be called after the given delay.
		/// </summary>
		/// <param name="delay"></param>
		/// <param name="callback"></param>
		public void Schedule(TimeSpan delay, Action callback)
			=> this.Schedule(delay, TimeSpan.Zero, callback);

		/// <summary>
		/// Schedules callback to be called after the given delay.
		/// </summary>
		/// <param name="delay"></param>
		/// <param name="repeatDelay"></param>
		/// <param name="callback"></param>
		public void Schedule(TimeSpan delay, TimeSpan repeatDelay, Action callback)
		{
			lock (_pending)
			{
				_pending.Add(new ScheduledCallback(delay, repeatDelay, callback));
				_newElements = true;

				if (delay.TotalMilliseconds <= HotLoopThreshold.TotalMilliseconds * 2)
					_wakeFromColdLoop.Set();
			}
		}

		/// <summary>
		/// Constant loop that manages and executes the scheduled callbacks.
		/// </summary>
		/// <param name="_"></param>
		private void Loop(object _)
		{
			var timer = Stopwatch.StartNew();
			var nextCallback = (ScheduledCallback)null;

			while (!_disposing)
			{
				_allowed.WaitOne();

				if (nextCallback != null && timer.Elapsed >= nextCallback?.Delay)
				{
					var elapsed = timer.Elapsed;
					timer.Restart();

					foreach (var item in _scheduled)
					{
						item.Delay -= elapsed;

						if (item.Delay <= TimeSpan.Zero)
							_execute.Add(item);
					}

					if (_execute.Count > 0)
					{
						foreach (var item in _execute)
						{
							item.Callback?.Invoke();

							if (item.RepeatDelay == TimeSpan.Zero)
							{
								_scheduled.Remove(item);
							}
							else
							{
								item.Delay = item.RepeatDelay;
								_newElements = true;
							}
						}

						_execute.Clear();
					}

					nextCallback = _scheduled.FirstOrDefault();
				}

				if (_newElements)
				{
					lock (_pending)
					{
						_newElements = false;

						if (_pending.Count != 0)
						{
							_scheduled.AddRange(_pending);
							_pending.Clear();
						}

						_scheduled.Sort();
						nextCallback = _scheduled.FirstOrDefault();
					}
				}

				if (nextCallback == null || nextCallback.Delay > HotLoopThreshold)
					_wakeFromColdLoop.WaitOne(1);
				else
					_wakeFromColdLoop.WaitOne(0);
			}
		}

		/// <summary>
		/// Disposes the scheduler, stopping any callback handling.
		/// </summary>
		public void Dispose()
		{
			_disposing = true;
		}
	}
}
