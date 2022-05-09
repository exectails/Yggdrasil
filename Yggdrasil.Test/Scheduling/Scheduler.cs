using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using Xunit;
using Yggdrasil.Scheduling;

namespace Yggdrasil.Test.Scheduling
{
	public class SchedulerTests
	{
		[Fact]
		public void Schedule()
		{
			var scheduler = new Scheduler();
			scheduler.Start();

			var tolerance = 20;

			TestSchedule(scheduler, 2000, tolerance);
			TestSchedule(scheduler, 1000, tolerance);
			TestSchedule(scheduler, 400, tolerance);

			scheduler.Dispose();
		}

		private static void TestSchedule(Scheduler scheduler, int delay, int tolerance)
		{
			var then = DateTime.Now;
			var now = DateTime.Now;

			scheduler.Schedule(delay, _ => now = DateTime.Now);
			Thread.Sleep(delay + tolerance * 3);
			Assert.InRange((now - then).TotalMilliseconds, delay - tolerance, delay + tolerance);
		}

		[Fact]
		public void ScheduleRepeat()
		{
			var scheduler = new Scheduler();
			scheduler.Start();

			var calls = 0;
			var tolerance = 50;

			scheduler.Schedule(TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(500), _ => calls++);
			Assert.Equal(0, calls);

			Thread.Sleep(1000 + tolerance);
			Assert.Equal(1, calls);

			for (var i = 2; i <= 5; ++i)
			{
				Thread.Sleep(500 + tolerance);
				Assert.Equal(i, calls);
			}

			scheduler.Dispose();
		}

		[Fact]
		public void Cancel()
		{
			var scheduler = new Scheduler();
			scheduler.Start();

			var waitTolerance = 100;

			var called = false;
			var id = scheduler.Schedule(TimeSpan.FromMilliseconds(1000), _ => called = true);
			Assert.Equal(false, called);

			Thread.Sleep(1000 + waitTolerance);
			Assert.Equal(true, called);

			called = false;
			id = scheduler.Schedule(TimeSpan.FromMilliseconds(1000), _ => called = true);
			Assert.Equal(false, called);

			Thread.Sleep(500);
			scheduler.Cancel(id);
			Thread.Sleep(1000 + waitTolerance);
			Assert.Equal(false, called);
			Thread.Sleep(1000 + waitTolerance);
			Assert.Equal(false, called);

			scheduler.Dispose();
		}

		[Fact]
		public void Arguments()
		{
			var scheduler = new Scheduler();
			scheduler.Start();

			var waitTolerance = 100;
			var x = 0;

			scheduler.Schedule(1000, state =>
			{
				x = (int)state.Arguments[0];
			},
			100);

			Assert.Equal(0, x);
			Thread.Sleep(1000 + waitTolerance);
			Assert.Equal(100, x);

			scheduler.Schedule(1000, state =>
			{
				x = (int)state.Arguments[0] * 2;
			},
			200);

			Assert.Equal(100, x);
			Thread.Sleep(1000 + waitTolerance);
			Assert.Equal(400, x);

			scheduler.Dispose();
		}

		[Fact]
		public void Counter()
		{
			var scheduler = new Scheduler();
			scheduler.Start();

			var waitTolerance = 50;
			var x = 0L;

			scheduler.Schedule(500, 500, state => x = state.ExecuteCount);

			Assert.Equal(0, x);
			Thread.Sleep(500 + waitTolerance);
			Assert.Equal(1, x);
			Thread.Sleep(1000 + waitTolerance);
			Assert.Equal(3, x);

			scheduler.Dispose();
		}
	}
}
