using System.Collections.Generic;

namespace Yggdrasil.Extensions
{
	/// <summary>
	/// Extensions for the Queue class.
	/// </summary>
	public static class QueueExtensions
	{
		/// <summary>
		/// Adds all of the list's elements to the queue in order.
		/// </summary>
		/// <typeparam name="TElement"></typeparam>
		/// <param name="queue"></param>
		/// <param name="list"></param>
		public static void AddRange<TElement>(this Queue<TElement> queue, IList<TElement> list)
		{
			for (var i = 0; i < list.Count; ++i)
			{
				var item = list[i];
				queue.Enqueue(item);
			}
		}

		/// <summary>
		/// Adds all of the list's elements to the queue in order.
		/// </summary>
		/// <typeparam name="TElement"></typeparam>
		/// <param name="queue"></param>
		/// <param name="list"></param>
		public static void AddRange<TElement>(this Queue<TElement> queue, IEnumerable<TElement> list)
		{
			foreach (var item in list)
				queue.Enqueue(item);
		}
	}
}
