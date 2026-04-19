using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Yggdrasil.Collections
{
	/// <summary>
	/// A List implementation that takes its lists from a shared pool to
	/// minimize allocations.
	/// </summary>
	/// <remarks>
	/// The class functions the exact same as a normal List, except that
	/// its instances are taken from a shared pool. After use, the list
	/// needs to be disposed, so it can be returned to the pool. It's
	/// intended for temporary lists used in hot paths and should not be
	/// used for long-term storage.
	/// </remarks>
	/// <typeparam name="T"></typeparam>
	public class PooledList<T> : List<T>, IDisposable
	{
		private static readonly ConcurrentBag<PooledList<T>> Pool = new ConcurrentBag<PooledList<T>>();
		private const int MaxPoolSize = 5000;
		private const int MaxCapacity = 1024;
		private static int PoolCount;

		private bool _disposed;

		/// <summary>
		/// Prevent external instantiation.
		/// </summary>
		private PooledList() : base()
		{
		}

		/// <summary>
		/// Returns a list from the pool.
		/// </summary>
		/// <remarks>
		/// It's highly recommended to use this method in a using
		/// statement, so the list will be returned to the pool
		/// semi-automatically.
		/// </remarks>
		/// <returns></returns>
		public static PooledList<T> Rent()
		{
			if (Pool.TryTake(out var list))
				Interlocked.Decrement(ref PoolCount);
			else
				list = new PooledList<T>();

			list._disposed = false;
			return list;
		}

		/// <summary>
		/// Disposes the list and returns it to the pool. After calling
		/// this method, the list should not be used anymore.
		/// </summary>
		public void Dispose()
		{
			if (_disposed)
				return;

			_disposed = true;

			if (this.Capacity >= MaxCapacity)
			{
				this.Clear();
				return;
			}

			this.Clear();

			// Return the list to the pool up to the max pool size
			if (Interlocked.Increment(ref PoolCount) <= MaxPoolSize)
				Pool.Add(this);
			else
				Interlocked.Decrement(ref PoolCount);
		}
	}
}
