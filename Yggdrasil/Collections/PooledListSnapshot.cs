using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Yggdrasil.Collections
{
	/// <summary>
	/// Provides a snapshot of a collection using a pooled array to
	/// minimize allocations while allowing elements to be selectively
	/// added.
	/// </summary>
	/// <remarks>
	/// Intended for scenarios where a temporary snapshot of a collection
	/// needs to be returned. The snapshot is read-only and needs to be
	/// diposed to return the rented resources to the pool. The caller
	/// must ensure that the snapshot is not used after disposal, and that
	/// the source collection is not modified concurrently while the
	/// snapshot is being created or used.
	/// </remarks>
	/// <typeparam name="T"></typeparam>
	public ref struct PooledListSnapshot<T>
	{
		private T[] _rentedArray;
		private int _count;

		/// <summary>
		/// Returns a read-only span over the snapshot data. The span is
		/// only valid while the snapshot is in scope and undisposed.
		/// </summary>
		public ReadOnlySpan<T> Span
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => new ReadOnlySpan<T>(_rentedArray, 0, _count);
		}

		/// <summary>
		/// Adds the given item to the snapshot.
		/// </summary>
		/// <param name="item"></param>
		public void Add(T item)
		{
			if (_rentedArray == null)
			{
				_rentedArray = ArrayPool<T>.Shared.Rent(16);
			}
			else if (_count >= _rentedArray.Length)
			{
				var newArray = ArrayPool<T>.Shared.Rent(_rentedArray.Length * 2);
				Array.Copy(_rentedArray, newArray, _count);

				ArrayPool<T>.Shared.Return(_rentedArray, clearArray: true);
				_rentedArray = newArray;
			}

			_rentedArray[_count++] = item;
		}

		/// <summary>
		/// Disposes the snapshot, returning the rented resources to the
		/// pool.
		/// </summary>
		public void Dispose()
		{
			if (_rentedArray != null)
			{
				ArrayPool<T>.Shared.Return(_rentedArray, clearArray: true);
				_rentedArray = null;
			}

			_count = 0;
		}

		/// <summary>
		/// Returns an enumerator for the snapshot's span.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ReadOnlySpan<T>.Enumerator GetEnumerator()
		{
			return this.Span.GetEnumerator();
		}

		/// <summary>
		/// Returns a copy of the snapshot data as a newly allocated
		/// array.
		/// </summary>
		/// <returns></returns>
		public T[] ToArray()
		{
			return this.Span.ToArray();
		}

		/// <summary>
		/// Returns a copy of the snapshot data as a newly allocated list.
		/// </summary>
		/// <returns></returns>
		public List<T> ToList()
		{
			var result = new List<T>(_count);

			foreach (var item in this.Span)
				result.Add(item);

			return result;
		}

		/// <summary>
		/// Adds the snapshot data to the given list, modifying the list
		/// in-place.
		/// </summary>
		/// <param name="destination"></param>
		public void ToList(List<T> destination)
		{
			if (_count == 0)
				return;

			if (destination.Capacity < destination.Count + _count)
				destination.Capacity = destination.Count + _count;

			foreach (var item in this.Span)
				destination.Add(item);
		}
	}
}
