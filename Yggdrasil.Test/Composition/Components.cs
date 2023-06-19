using System;
using Xunit;
using Yggdrasil.Composition;
using Yggdrasil.Scheduling;

namespace Yggdrasil.Test.Composition
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
		public void RemoveByReference()
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
		public void RemoveByGeneric()
		{
			var components = new ComponentCollection();

			Assert.False(components.TryGet<Component1>(out _));
			Assert.Null(components.Get<Component1>());

			components.Add(new Component1());
			Assert.True(components.TryGet<Component1>(out _));
			Assert.Equal(255, components.Get<Component1>().Foo);

			components.Remove<Component1>();
			Assert.False(components.TryGet<Component1>(out _));
			Assert.Null(components.Get<Component1>());
		}

		[Fact]
		public void Has()
		{
			var components = new ComponentCollection();

			Assert.False(components.TryGet<Component1>(out _));
			Assert.Null(components.Get<Component1>());

			Assert.False(components.Has<Component1>());
			Assert.False(components.Has<Component2>());

			components.Add(new Component1());
			Assert.True(components.Has<Component1>());
			Assert.False(components.Has<Component2>());

			components.Remove<Component1>();
			Assert.False(components.Has<Component1>());
			Assert.False(components.Has<Component2>());
			Assert.False(components.Has<Component2>());

			components.Add(new Component2());
			Assert.True(components.Has<Component2>());
			Assert.False(components.Has<Component1>());

			components.Remove<Component2>();
			Assert.False(components.Has<Component2>());
			Assert.False(components.Has<Component1>());
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

		private class Component1 : IComponent, IUpdateable
		{
			public int Foo { get; private set; } = 255;

			public void Update(TimeSpan elapsed)
			{
				this.Foo++;
			}
		}

		private class Component2 : IComponent, IUpdateable
		{
			public int Foo { get; private set; } = 32767;

			public void Update(TimeSpan elapsed)
			{
				this.Foo++;
			}
		}
	}
}
