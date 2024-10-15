using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Yggdrasil.Events;

namespace Yggdrasil.Test.Events
{
	public class OnAttributeTests
	{
		[Fact]
		public void SubscribeAndRaise()
		{
			var server = new ServerEvents();
			var client = new ClientEvents();

			OnAttribute.Load(client, server);

			server.OnFoo(server, EventArgs.Empty);
			server.Bar.Raise(server, EventArgs.Empty);

			Assert.True(client.FooCalled);
			Assert.True(client.BarCalled);
		}

		[Fact]
		public void Unsubscribe()
		{
			var server = new ServerEvents();
			var client = new ClientEvents();

			OnAttribute.Load(client, server);

			server.OnFoo(server, EventArgs.Empty);
			server.Bar.Raise(server, EventArgs.Empty);

			Assert.True(client.FooCalled);
			Assert.True(client.BarCalled);
			client.FooCalled = false;
			client.BarCalled = false;

			OnAttribute.Unload(client, server);

			server.OnFoo(server, EventArgs.Empty);
			server.Bar.Raise(server, EventArgs.Empty);

			Assert.False(client.FooCalled);
			Assert.False(client.BarCalled);
		}

		[Fact]
		public void FailSubscribe()
		{
			var server = new ServerEvents();
			var client1 = new MissingClientEvents();

			Assert.Throws<SubscriptionException>(() => OnAttribute.Load(client1, server));

			var client2 = new WrongSignatureClientEvents();

			Assert.Throws<SubscriptionException>(() => OnAttribute.Load(client2, server));
		}

		private class ServerEvents
		{
			public event EventHandler<EventArgs> Foo;
			public void OnFoo(object sender, EventArgs e) => Foo?.Invoke(sender, e);

			public readonly Event<EventArgs> Bar = new();
		}

		private class ClientEvents
		{
			public bool FooCalled;
			public bool BarCalled;

			[On("Foo")]
			public void OnFoo(object sender, EventArgs e)
				=> FooCalled = true;

			[On("Bar")]
			public void OnBar(object sender, EventArgs e)
				=> BarCalled = true;
		}

		private class MissingClientEvents
		{
			public bool Foo2Called;

			[On("Foo2")]
			public void OnFoo2(object sender, EventArgs e)
				=> Foo2Called = true;
		}

		private class WrongSignatureClientEvents
		{
			public bool Foo3Called;

			[On("Foo3")]
			public void OnFoo3(object sender, DummyEventArgs e)
				=> Foo3Called = true;
		}

		private class DummyEventArgs : EventArgs
		{
		}
	}
}
