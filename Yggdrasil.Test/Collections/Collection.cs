// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Xunit;
using Yggdrasil.Collections;

namespace Yggdrasil.Test.Collections
{
	public class CollectionTests
	{
		[Fact]
		public void AddingAndGetting()
		{
			var col = new Collection<int, int>();

			Assert.Equal(true, col.Add(1, 1000));
			Assert.Equal(true, col.Add(2, 2000));
			Assert.Equal(true, col.Add(3, 3000));
			Assert.Equal(false, col.Add(3, 3333));
			Assert.Equal(true, col.Add(4, 4000));

			Assert.Equal(4, col.Count);
			Assert.Equal(1000, col.Get(1));
			Assert.Equal(2000, col.Get(2));
			Assert.Equal(3000, col.Get(3));
			Assert.Equal(4000, col.Get(4));
		}

		[Fact]
		public void SettingAndGetting()
		{
			var col = new Collection<int, int>();

			col.Set(1, 1000);
			col.Set(2, 2000);
			col.Set(3, 3000);
			col.Set(3, 3333);
			col.Set(4, 4000);

			Assert.Equal(4, col.Count);
			Assert.Equal(1000, col.Get(1));
			Assert.Equal(2000, col.Get(2));
			Assert.Equal(3333, col.Get(3));
			Assert.Equal(4000, col.Get(4));
		}

		[Fact]
		public void Removing()
		{
			var col = new Collection<int, int>();

			col.Set(1, 1000);
			col.Set(2, 2000);
			col.Set(3, 3000);
			col.Set(4, 4000);

			col.Remove(2);
			col.Remove(3);

			Assert.Equal(2, col.Count);
			Assert.Equal(1000, col.Get(1));
			Assert.Equal(4000, col.Get(4));
		}

		[Fact]
		public void Clearing()
		{
			var col = new Collection<int, int>();

			col.Set(1, 1000);
			col.Set(2, 2000);
			col.Set(3, 3000);
			col.Set(4, 4000);

			col.Clear();

			Assert.Equal(0, col.Count);
			Assert.Equal(0, col.Get(1));
			Assert.Equal(0, col.Get(2));
			Assert.Equal(0, col.Get(3));
			Assert.Equal(0, col.Get(4));
		}

		[Fact]
		public void Containing()
		{
			var col = new Collection<int, int>();

			col.Set(1, 1000);
			col.Set(2, 2000);
			col.Set(3, 3000);
			col.Set(4, 4000);

			Assert.Equal(true, col.ContainsKey(1));
			Assert.Equal(true, col.ContainsKey(3));
			Assert.Equal(false, col.ContainsKey(5));
			Assert.Equal(true, col.ContainsValue(2000));
			Assert.Equal(true, col.ContainsValue(4000));
			Assert.Equal(false, col.ContainsValue(6000));
		}
	}
}
