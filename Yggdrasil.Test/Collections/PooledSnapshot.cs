using System;
using System.Collections.Generic;
using Xunit;
using Yggdrasil.Collections;

namespace Yggdrasil.Test.Collections
{
	public class PooledSnapshotTests
	{
		[Fact]
		public void EmptyCollection_CreatesEmptySnapshot()
		{
			var source = new List<int>();
			var snapshot = new PooledSnapshot<int>(source);

			Assert.True(snapshot.Span.IsEmpty);
			Assert.Empty(snapshot.ToArray());
			Assert.Empty(snapshot.ToList());

			// Should not throw on dispose or double-dispose
			snapshot.Dispose();
			snapshot.Dispose();
		}

		[Fact]
		public void PopulatedCollection_MaintainsDataAndOrder()
		{
			var source = new List<int>() { 1, 2, 3 };
			var snapshot = new PooledSnapshot<int>(source);

			Assert.Equal(3, snapshot.Span.Length);
			Assert.Equal(1, snapshot.Span[0]);
			Assert.Equal(2, snapshot.Span[1]);
			Assert.Equal(3, snapshot.Span[2]);

			snapshot.Dispose();
		}

		[Fact]
		public void DoubleDispose_DoesNotThrow()
		{
			var source = new List<int>() { 1, 2, 3 };
			var snapshot = new PooledSnapshot<int>(source);

			snapshot.Dispose();

			// Should not throw
			snapshot.Dispose();
		}

		[Fact]
		public void AccessSpanAfterDispose_LogsExceptionDueToNullArrayWithCount()
		{
			var source = new List<int>() { 1, 2, 3 };
			var snapshot = new PooledSnapshot<int>(source);

			snapshot.Dispose();

			// Accessing a span with a null reference but a count > 0 will
			// throw
			var threw = false;
			try
			{
				var span = snapshot.Span;
			}
			catch (ArgumentOutOfRangeException)
			{
				threw = true;
			}

			Assert.True(threw);
		}

		[Fact]
		public void Conversions_ReturnCorrectData()
		{
			var source = new List<int>() { 4, 5, 6 };
			var snapshot = new PooledSnapshot<int>(source);

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
			var source = new List<int>() { 7, 8 };
			var snapshot = new PooledSnapshot<int>(source);

			var result = new List<int>();
			foreach (var item in snapshot)
				result.Add(item);


			Assert.Equal(source, result);

			snapshot.Dispose();
		}
	}
}
