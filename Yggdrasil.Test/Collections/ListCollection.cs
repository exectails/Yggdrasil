// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Xunit;
using Yggdrasil.Collections;

namespace Yggdrasil.Test.Collections
{
	public class ListCollectionTests
	{
		[Fact]
		public void AddingAndGetting()
		{
			var col = new ListCollection<int, int>();

			for (int i = 0; i < 5; ++i)
				col.Add(1, 1000 + i);

			for (int i = 0; i < 5; ++i)
				col.Add(2, 2200 + i);

			var l1 = col.Get(1);
			var l2 = col.Get(2);

			Assert.Equal(new int[] { 1000, 1001, 1002, 1003, 1004 }, l1.ToArray());
			Assert.Equal(new int[] { 2200, 2201, 2202, 2203, 2204 }, l2.ToArray());
		}

		[Fact]
		public void Clearing()
		{
			var col = new ListCollection<int, int>();

			for (int i = 0; i < 5; ++i)
				col.Add(1, 1000 + i);

			for (int i = 0; i < 5; ++i)
				col.Add(2, 2200 + i);

			col.Clear();
			var l1 = col.Get(1);
			var l2 = col.Get(2);

			Assert.Equal(null, l1);
			Assert.Equal(null, l2);
		}

		[Fact]
		public void ListClearing()
		{
			var col = new ListCollection<int, int>();

			for (int i = 0; i < 5; ++i)
				col.Add(1, 1000 + i);

			for (int i = 0; i < 5; ++i)
				col.Add(2, 2200 + i);

			var l1 = col.Get(1);
			var l2 = col.Get(2);

			Assert.Equal(new int[] { 1000, 1001, 1002, 1003, 1004 }, l1.ToArray());
			Assert.Equal(new int[] { 2200, 2201, 2202, 2203, 2204 }, l2.ToArray());

			col.Clear(1);
			var l3 = col.Get(1);
			var l4 = col.Get(2);

			Assert.Equal(new int[] { }, l3.ToArray());
			Assert.Equal(new int[] { 2200, 2201, 2202, 2203, 2204 }, l4.ToArray());

			col.Clear(2);
			var l5 = col.Get(1);
			var l6 = col.Get(2);

			Assert.Equal(new int[] { }, l5.ToArray());
			Assert.Equal(new int[] { }, l6.ToArray());
		}

		[Fact]
		public void Containing()
		{
			var col = new ListCollection<int, int>();

			for (int i = 0; i < 5; ++i)
				col.Add(1, 1000 + i);

			for (int i = 0; i < 5; ++i)
				col.Add(2, 2200 + i);

			Assert.Equal(true, col.ContainsKey(1));
			Assert.Equal(true, col.ContainsKey(2));
			Assert.Equal(false, col.ContainsKey(3));
			Assert.Equal(false, col.ContainsKey(4));
		}
	}
}
