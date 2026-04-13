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
			Assert.True(ev.HasSubscribers);
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
			Assert.True(ev.HasSubscribers);
			eventRaised = false;

			ev.Unsubscribe(handler);
			ev.Raise(EventArgs.Empty);

			Assert.False(eventRaised);
			Assert.False(ev.HasSubscribers);
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

		[Fact]
		public void BadReadOnlyTest()
		{
			var foo = new Foo1();

			var fooRaised = false;

			void fooHandler(object sender, EventArgs e) => fooRaised = true;

			foo.Foo.Subscribe(fooHandler);
			foo.Foo.Raise(EventArgs.Empty);

			Assert.False(fooRaised);
		}

		[Fact]
		public void BadPropertyTest()
		{
			var foo = new Foo2();

			var fooRaised = false;

			void fooHandler(object sender, EventArgs e) => fooRaised = true;

			foo.Foo.Subscribe(fooHandler);
			foo.Foo.Raise(EventArgs.Empty);

			Assert.False(fooRaised);
		}

		private class EventManager
		{
			public Event<EventArgs> Foo;
			public Event<EventArgs> Bar;
		}

		private class Foo1
		{
			public readonly Event<EventArgs> Foo;
		}

		private class Foo2
		{
			public Event<EventArgs> Foo { get; set; }
		}
	}
}
