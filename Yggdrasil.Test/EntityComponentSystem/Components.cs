using System;
using Xunit;
using Yggdrasil.EntityComponentSystem;

namespace Yggdrasil.Test.EntityComponentSystem
{
	public class Components
	{
		[Fact]
		public void Add()
		{
			var components = new ComponentCollection();

			components.Add(new Component1());
			Assert.Equal(255, components.Get<Component1>().Foo);
		}

		[Fact]
		public void Remove()
		{
			var components = new ComponentCollection();

			Assert.False(components.TryGet<Component1>(out _));
			Assert.Null(components.Get<Component1>());

			components.Add(new Component1());
			Assert.True(components.TryGet<Component1>(out _));
			Assert.Equal(255, components.Get<Component1>().Foo);

			components.Remove(components.Get<Component1>());
			Assert.False(components.TryGet<Component1>(out _));
			Assert.Null(components.Get<Component1>());
		}

		[Fact]
		public void Update()
		{
			var components = new ComponentCollection();

			components.Add(new Component1());
			Assert.Equal(255, components.Get<Component1>().Foo);

			for (var i = 0; i < 5; ++i)
			{
				components.Update(TimeSpan.FromMilliseconds(16.7));
			}

			Assert.Equal(260, components.Get<Component1>().Foo);
		}

		private class Component1 : IUpdatableComponent
		{
			public int Foo { get; private set; } = 255;

			public void Update(TimeSpan elapsed)
			{
				this.Foo++;
			}
		}
	}
}
