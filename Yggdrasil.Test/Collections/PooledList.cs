using System.Collections.Generic;
using Xunit;
using Yggdrasil.Collections;

namespace Yggdrasil.Test.Collections
{
	public class PooledListTests
	{
		[Fact]
		public void Rent_ReturnsEmptyList()
		{
			var list = PooledList<int>.Rent();
			Assert.Empty(list);
			list.Add(1);
			list.Dispose();

			var list2 = PooledList<int>.Rent();
			Assert.Empty(list2);
			list2.Dispose();
		}

		[Fact]
		public void Dispose_ShouldNotBreakIfTriggeredTwice()
		{
			var list1 = PooledList<int>.Rent();
			list1.Dispose();
			list1.Dispose(); // Shouldn't throw and shouldn't double-return

			var list2 = PooledList<int>.Rent();
			var list3 = PooledList<int>.Rent();

			// We can't objectively count the pool, but we can verify it
			// doesn't give us list1 back twice concurrently
			Assert.NotSame(list2, list3);

			list2.Dispose();
			list3.Dispose();
		}

		[Fact]
		public void Dispose_ListOverMaxCapacity_Discarded()
		{
			var list = PooledList<int>.Rent();
			list.Capacity = 2048; // Artificial inflation over 1024
			list.Add(1);

			list.Dispose();

			// Because it was discarded, renting should provide a new list
			var list2 = PooledList<int>.Rent();
			Assert.NotSame(list, list2);
			list2.Dispose();
		}

		[Fact]
		public void Dispose_PoolFull_ExtraDiscarded()
		{
			var itemsToRent = 5005;
			var lists = new List<PooledList<int>>(itemsToRent);

			// Rent and return 5005 lists, which is over the pool max size
			for (var i = 0; i < itemsToRent; ++i)
				lists.Add(PooledList<int>.Rent());

			for (var i = 0; i < itemsToRent; ++i)
				lists[i].Dispose();

			// We shouldn't crash, the extra 5 just get thrown away
			var finalList = PooledList<int>.Rent();
			Assert.NotNull(finalList);
			finalList.Dispose();
		}
	}
}
