using System.Collections.Generic;
using Xunit;
using Yggdrasil.Collections;

namespace Yggdrasil.Test.Collections
{
	public class PooledListSnapshotTests
	{
		[Fact]
		public void EmptySnapshot_IsProperlyEmpty()
		{
			var snapshot = new PooledListSnapshot<int>();

			Assert.True(snapshot.Span.IsEmpty);
			Assert.Empty(snapshot.ToArray());
			Assert.Empty(snapshot.ToList());

			// Should not throw on dispose or double-dispose
			snapshot.Dispose();
			snapshot.Dispose();
		}

		[Fact]
		public void AddItems_MaintainsDataAndOrder()
		{
			var snapshot = new PooledListSnapshot<int>();
			snapshot.Add(1);
			snapshot.Add(2);
			snapshot.Add(3);

			Assert.Equal(3, snapshot.Span.Length);
			Assert.Equal(1, snapshot.Span[0]);
			Assert.Equal(2, snapshot.Span[1]);
			Assert.Equal(3, snapshot.Span[2]);

			snapshot.Dispose();
		}

		[Fact]
		public void AddItems_TriggersResize_MaintainsData()
		{
			var snapshot = new PooledListSnapshot<int>();

			// Default initial capacity is 16. Setting 20 items will
			// trigger a resize
			for (var i = 0; i < 20; ++i)
				snapshot.Add(i);

			Assert.Equal(20, snapshot.Span.Length);
			for (var i = 0; i < 20; ++i)
				Assert.Equal(i, snapshot.Span[i]);

			snapshot.Dispose();
		}

		[Fact]
		public void DoubleDispose_DoesNotThrow()
		{
			var snapshot = new PooledListSnapshot<int>();
			snapshot.Add(1);

			snapshot.Dispose();

			// Should not throw
			snapshot.Dispose();
		}

		[Fact]
		public void AccessSpanAfterDispose_ReturnsEmptySpan()
		{
			var snapshot = new PooledListSnapshot<int>();
			snapshot.Add(1);
			snapshot.Add(2);

			snapshot.Dispose();

			// Because _count is set to 0 in Dispose(), this safely
			// returns an empty span
			Assert.True(snapshot.Span.IsEmpty);
		}

		[Fact]
		public void Conversions_ReturnCorrectData()
		{
			var snapshot = new PooledListSnapshot<int>();
			snapshot.Add(4);
			snapshot.Add(5);
			snapshot.Add(6);

			var array = snapshot.ToArray();
			Assert.Equal(new[] { 4, 5, 6 }, array);

			var list = snapshot.ToList();
			Assert.Equal([4, 5, 6], list);

			var dest = new List<int>() { 1, 2, 3 };
			snapshot.ToList(dest);
			Assert.Equal([1, 2, 3, 4, 5, 6], dest);

			snapshot.Dispose();
		}

		[Fact]
		public void Enumerator_WorksInForeach()
		{
			var snapshot = new PooledListSnapshot<int>();
			snapshot.Add(7);
			snapshot.Add(8);

			var result = new List<int>();
			foreach (var item in snapshot)
				result.Add(item);

			Assert.Equal([7, 8], result);

			snapshot.Dispose();
		}
	}
}
