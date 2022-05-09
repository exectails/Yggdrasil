// https://stackoverflow.com/a/55676794/1171898
// License: public domain (no restrictions or obligations)
// Author: Vitaly Vinogradov

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Yggdrasil.Scheduling
{
	/// <summary>
	/// High resolution scheduler. 
	/// </summary>
	public class Scheduler : IDisposable
	{
		private readonly static TimeSpan HotLoopThreshold = TimeSpan.FromMilliseconds(32);

		private readonly Thread _loopThread;

		private readonly ManualResetEvent _allowed = new ManualResetEvent(false);
		private readonly AutoResetEvent _wakeFromColdLoop = new AutoResetEvent(false);

		private readonly List<ScheduledCallback> _scheduled = new List<ScheduledCallback>();
		private readonly List<ScheduledCallback> _pending = new List<ScheduledCallback>();
		private readonly List<ScheduledCallback> _execute = new List<ScheduledCallback>();
		private readonly HashSet<long> _cancelled = new HashSet<long>();
		private long _ids;

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
		/// <param name="delayMs"></param>
		/// <param name="callback"></param>
		/// <param name="args"></param>
		public long Schedule(double delayMs, ScheduledCallbackFunc callback, params object[] args)
			=> this.Schedule(TimeSpan.FromMilliseconds(delayMs), TimeSpan.Zero, callback, args);

		/// <summary>
		/// Schedules callback to be called after the given delay.
		/// </summary>
		/// <param name="delayMs"></param>
		/// <param name="repeatDelayMs"></param>
		/// <param name="callback"></param>
		/// <param name="args"></param>
		public long Schedule(double delayMs, int repeatDelayMs, ScheduledCallbackFunc callback, params object[] args)
			=> this.Schedule(TimeSpan.FromMilliseconds(delayMs), TimeSpan.FromMilliseconds(repeatDelayMs), callback, args);

		/// <summary>
		/// Schedules callback to be called after the given delay.
		/// </summary>
		/// <param name="delay"></param>
		/// <param name="callback"></param>
		/// <param name="args"></param>
		public long Schedule(TimeSpan delay, ScheduledCallbackFunc callback, params object[] args)
			=> this.Schedule(delay, TimeSpan.Zero, callback, args);

		/// <summary>
		/// Schedules callback to be called after the given delay.
		/// </summary>
		/// <param name="delay"></param>
		/// <param name="repeatDelay"></param>
		/// <param name="callback"></param>
		/// <param name="args"></param>
		public long Schedule(TimeSpan delay, TimeSpan repeatDelay, ScheduledCallbackFunc callback, params object[] args)
		{
			lock (_pending)
			{
				var newId = ++_ids;

				_pending.Add(new ScheduledCallback(newId, delay, repeatDelay, callback, args));
				_newElements = true;

				if (delay.TotalMilliseconds <= HotLoopThreshold.TotalMilliseconds * 2)
					_wakeFromColdLoop.Set();

				return newId;
			}
		}

		/// <summary>
		/// Cancels the scheduled callback with the given id.
		/// </summary>
		/// <param name="id"></param>
		public void Cancel(long id)
		{
			lock (_cancelled)
				_cancelled.Add(id);
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

				var elapsed = timer.Elapsed;
				timer.Restart();

				// In the original, the elapsed time grab and the restart
				// happened inside this if, but that caused callbacks to
				// fire early, because if nothing was scheduled, the Stop-
				// Watch still kept ticking, and when something was added,
				// the full elapsed time since the last execution would be
				// subtracted from the new callback's delay.
				// It's almost as if you can't just copy random code from
				// StackOverflow and expect it to work. Unbelievable!
				if (nextCallback != null /*&& timer.Elapsed >= nextCallback.Delay*/)
				{
					foreach (var item in _scheduled)
					{
						item.Delay -= elapsed;
						item.Elapsed += elapsed;

						if (item.Delay <= TimeSpan.Zero)
							_execute.Add(item);
					}

					if (_execute.Count > 0)
					{
						foreach (var item in _execute)
						{
							// What's more efficient, not meddling with the
							// list until absolutely necessary or keeping
							// even cancelled timers around until they fire
							// the next time? Not sure.
							lock (_cancelled)
							{
								if (_cancelled.Contains(item.Id))
								{
									_scheduled.Remove(item);
									_cancelled.Remove(item.Id);
									continue;
								}
							}

							item.ExecuteCount++;
							item.Callback?.Invoke(new CallbackState(item.Elapsed, item.ExecuteCount, item.Arguments));

							if (item.RepeatDelay == TimeSpan.Zero)
							{
								_scheduled.Remove(item);
							}
							else
							{
								item.Delay = item.RepeatDelay;
								item.Elapsed = TimeSpan.Zero;
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
