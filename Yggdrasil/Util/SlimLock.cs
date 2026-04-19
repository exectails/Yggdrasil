using System.Threading;

namespace Yggdrasil.Util
{
	/// <summary>
	/// A helper struct for using ReaderWriterLockSlim with a using
	/// statement.
	/// </summary>
	public readonly ref struct SlimLock
	{
		private readonly ReaderWriterLockSlim _lock;
		private readonly bool _isWrite;

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="rwls"></param>
		/// <param name="isWrite"></param>
		private SlimLock(ReaderWriterLockSlim rwls, bool isWrite)
		{
			_lock = rwls;
			_isWrite = isWrite;
		}

		/// <summary>
		/// Enters a read lock and returns a new instance of SlimLock. The
		/// lock will be released when the instance is disposed.
		/// </summary>
		/// <param name="rwls"></param>
		/// <returns></returns>
		public static SlimLock Read(ReaderWriterLockSlim rwls)
		{
			rwls.EnterReadLock();
			return new SlimLock(rwls, false);
		}

		/// <summary>
		/// Enters a write lock and returns a new instance of SlimLock.
		/// The lock will be released when the instance is disposed.
		/// </summary>
		/// <param name="rwls"></param>
		/// <returns></returns>
		public static SlimLock Write(ReaderWriterLockSlim rwls)
		{
			rwls.EnterWriteLock();
			return new SlimLock(rwls, true);
		}

		/// <summary>
		/// Releases the lock. This method is called automatically when
		/// the instance is disposed.
		/// </summary>
		public void Dispose()
		{
			if (_isWrite)
				_lock.ExitWriteLock();
			else
				_lock.ExitReadLock();
		}
	}
}
