using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Yggdrasil.Logging;

namespace Yggdrasil.Scheduling
{
	/// <summary>
	/// A world's heartbeat, running a continues loop that other
	/// components can utilize.
	/// </summary>
	/// <remarks>
	/// The Scheduler and the Heartbeat class can technically be used
	/// for the same purposes, but the Scheduler is a precisely timed
	/// loop that aims to execute callbacks at the exact moments they
	/// were scheduled for, while the Heartbeat ticks in continues,
	/// but less precise, intervals. It also being more resource
	/// friendly makes it the better candidate for regularly
	/// occurring events, such as raising time events.
	/// </remarks>
	public class Heartbeat
	{
		private bool _running = false;
		private bool _stopRequested = false;
		private Thread _heartbeatThread;

		private readonly List<IUpdateable> _updateables = new List<IUpdateable>();

		private readonly int _updatesPerSecondTarget;
		private TimeSpan _averageUpdateTickTotal;
		private int _averateUpdateTickCount;
		private TimeSpan _updatesTime;
		private int _updatesCount;

		/// <summary>
		/// Returns the average number of heartbeat updates per second.
		/// </summary>
		public int UpdatesPerSecond { get; private set; }

		/// <summary>
		/// Returns the average duration of the heartbeat's tick.
		/// </summary>
		public TimeSpan AverageUpdateTime { get; private set; }

		/// <summary>
		/// Raised every time the heartbeat executes.
		/// </summary>
		public event Action<TimeSpan> HeartbeatTick;

		/// <summary>
		/// Creates a new Heartbeat instance with a 10 updates per second
		/// target.
		/// </summary>
		public Heartbeat() : this(10)
		{
		}

		/// <summary>
		/// Creates a new Heartbeat instance with the given updates per
		/// second target.
		/// </summary>
		/// <param name="updatesPerSecondTarget"></param>
		public Heartbeat(int updatesPerSecondTarget)
		{
			_updatesPerSecondTarget = updatesPerSecondTarget;
		}

		/// <summary>
		/// Starts server's heartbeat.
		/// </summary>
		public void Start()
		{
			if (_running)
				throw new InvalidOperationException("Heartbeat was already started.");

			_heartbeatThread = new Thread(this.Loop);
			_heartbeatThread.Start();

			_running = true;
		}

		/// <summary>
		/// Stops heartbeat loop.
		/// </summary>
		public void Stop()
		{
			_stopRequested = true;
		}

		/// <summary>
		/// Adds object to automatic heartbeat update.
		/// </summary>
		/// <param name="updateable"></param>
		public void Add(IUpdateable updateable)
		{
			lock (_updateables)
			{
				if (!_updateables.Contains(updateable))
					_updateables.Add(updateable);
			}
		}

		/// <summary>
		/// Removes object from automatic heartbeat update.
		/// </summary>
		/// <param name="updateable"></param>
		public void Remove(IUpdateable updateable)
		{
			lock (_updateables)
				_updateables.Remove(updateable);
		}

		/// <summary>
		/// Endless heartbeat loop that runs updates and events.
		/// </summary>
		private void Loop()
		{
			var updateTimer = Stopwatch.StartNew();
			var updateDelay = 1000 / _updatesPerSecondTarget;

			while (true)
			{
				var elapsed = updateTimer.Elapsed;
				updateTimer.Restart();

				this.OnTick(elapsed);

				this.UpdateAverageUpdates(elapsed);
				this.UpdateAverageUpdateTime(updateTimer.Elapsed);

				if (_stopRequested)
				{
					_running = false;
					_stopRequested = false;
					return;
				}

				while (updateTimer.ElapsedMilliseconds < updateDelay)
					Thread.Sleep(1);
			}
		}

		/// <summary>
		/// Updates the average time the heartbeat update took.
		/// </summary>
		/// <param name="elapsed"></param>
		private void UpdateAverageUpdates(TimeSpan elapsed)
		{
			_updatesTime += elapsed;
			_updatesCount++;

			if (_updatesTime.TotalMilliseconds >= 1000)
			{
				this.UpdatesPerSecond = _updatesCount;

				_updatesTime = TimeSpan.Zero;
				_updatesCount = 0;
			}
		}

		/// <summary>
		/// Updates the average time the heartbeat update took.
		/// </summary>
		/// <param name="elapsed"></param>
		private void UpdateAverageUpdateTime(TimeSpan elapsed)
		{
			_averageUpdateTickTotal += elapsed;
			_averateUpdateTickCount++;

			if (_averateUpdateTickCount >= 10)
			{
				this.AverageUpdateTime = TimeSpan.FromTicks(_averageUpdateTickTotal.Ticks / _averateUpdateTickCount);

				_averageUpdateTickTotal = TimeSpan.Zero;
				_averateUpdateTickCount = 0;
			}
		}

		/// <summary>
		/// Callback for heartbeat timer. Called regularly to execute
		/// updates and events.
		/// </summary>
		/// <param name="elapsed"></param>
		private void OnTick(TimeSpan elapsed)
		{
			try
			{
				this.UpdateUpdateables(elapsed);
				this.RaiseEvents(elapsed);
			}
			catch (Exception ex)
			{
				Log.Error("Heartbeat.OnTick: " + ex);
			}
		}

		/// <summary>
		/// Runs updates on all registered updateable objects.
		/// </summary>
		/// <param name="elapsed"></param>
		private void UpdateUpdateables(TimeSpan elapsed)
		{
			for (var i = 0; i < _updateables.Count; ++i)
				_updateables[i].Update(elapsed);
		}

		/// <summary>
		/// Raises events as necessary.
		/// </summary>
		/// <param name="elapsed"></param>
		private void RaiseEvents(TimeSpan elapsed)
		{
			this.HeartbeatTick?.Invoke(elapsed);
		}
	}

	/// <summary>
	/// An object that can be updated.
	/// </summary>
	public interface IUpdateable
	{
		/// <summary>
		/// Updates object.
		/// </summary>
		/// <param name="elapsed"></param>
		void Update(TimeSpan elapsed);
	}
}
