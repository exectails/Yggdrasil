using System;
using Xunit;
using Yggdrasil.Events;

namespace Yggdrasil.Test.Events
{
	public class DynamicEventManagerTests
	{
		[Fact]
		public void SubscribeAndRaise()
		{
			var manager = new DynamicEventManager();
			var eventName = "test";
			var eventRaised = false;

			void handler(object sender, EventArgs e) => eventRaised = true;

			manager.Subscribe(eventName, handler);
			manager.Raise(eventName, EventArgs.Empty);

			Assert.True(eventRaised);
		}

		[Fact]
		public void RaiseNonExistent()
		{
			var manager = new DynamicEventManager();
			var eventName1 = "test";
			var eventName2 = "notExisting";
			var eventRaised = false;

			void handler(object sender, EventArgs e) => eventRaised = true;

			manager.Subscribe(eventName1, handler);
			manager.Raise(eventName2, EventArgs.Empty);

			Assert.False(eventRaised);
		}

		[Fact]
		public void Unsubscribe()
		{
			var manager = new DynamicEventManager();
			var eventName = "test";
			var eventRaised = false;

			void handler(object sender, EventArgs e) => eventRaised = true;

			manager.Subscribe(eventName, handler);
			manager.Raise(eventName, EventArgs.Empty);

			Assert.True(eventRaised);
			eventRaised = false;

			manager.Unsubscribe(eventName, handler);
			manager.Raise(eventName, EventArgs.Empty);

			Assert.False(eventRaised);
		}
	}
}
