using System;
using Xunit;
using Yggdrasil.Events;

namespace Yggdrasil.Test.Events
{
	public class EventTests
	{
		[Fact]
		public void SubscribeAndRaise()
		{
			var ev = new Event<EventArgs>();
			var eventRaised = false;

			void handler(object sender, EventArgs e) => eventRaised = true;

			ev.Subscribe(handler);
			ev.Raise(EventArgs.Empty);

			Assert.True(eventRaised);
		}

		[Fact]
		public void Unsubscribe()
		{
			var ev = new Event<EventArgs>();
			var eventRaised = false;

			void handler(object sender, EventArgs e) => eventRaised = true;

			ev.Subscribe(handler);
			ev.Raise(EventArgs.Empty);

			Assert.True(eventRaised);
			eventRaised = false;

			ev.Unsubscribe(handler);
			ev.Raise(EventArgs.Empty);

			Assert.False(eventRaised);
		}

		[Fact]
		public void EventManagerClass()
		{
			var manager = new EventManager();
			var fooRaised = false;
			var barRaised = false;

			void fooHandler(object sender, EventArgs e) => fooRaised = true;
			void barHandler(object sender, EventArgs e) => barRaised = true;

			manager.Foo.Subscribe(fooHandler);
			manager.Bar.Subscribe(barHandler);

			manager.Foo.Raise(EventArgs.Empty);
			manager.Bar.Raise(EventArgs.Empty);

			Assert.True(fooRaised);
			Assert.True(barRaised);
			fooRaised = false;
			barRaised = false;

			manager.Foo.Unsubscribe(fooHandler);

			manager.Foo.Raise(EventArgs.Empty);
			manager.Bar.Raise(EventArgs.Empty);

			Assert.False(fooRaised);
			Assert.True(barRaised);
			fooRaised = false;
			barRaised = false;

			manager.Bar.Unsubscribe(barHandler);

			manager.Foo.Raise(EventArgs.Empty);
			manager.Bar.Raise(EventArgs.Empty);

			Assert.False(fooRaised);
			Assert.False(barRaised);
		}

		private class EventManager
		{
			public readonly Event<EventArgs> Foo = new();
			public readonly Event<EventArgs> Bar = new();
		}
	}
}
