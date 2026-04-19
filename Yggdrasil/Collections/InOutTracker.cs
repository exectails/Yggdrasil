using System;
using System.Collections.Generic;

namespace Yggdrasil.Collections
{
	/// <summary>
	/// A helper class for tracking items that are added and removed from
	/// a list between updates, without allocations after initialization.
	/// </summary>
	/// <remarks>
	/// This class formalizes a pattern of tracking items that are added
	/// or removed from a list, such as visible actors or trigger areas.
	/// It's intended to be used exactly as designed and might not work as
	/// intended if used differently. Please refer to the example for how
	/// to use it correctly.
	/// </remarks>
	/// <example>
	/// items.Begin();
	///
	/// UpdateItems(items.UpdateList); items.Update();
	///
	/// if (items.Empty) { items.End(); return; }
	///
	/// foreach (var item in items.Added) { // ... }
	///
	/// foreach (var item in items.Removed) { // ... }
	///
	/// items.End();
	/// </example>
	/// <typeparam name="TItem"></typeparam>
	public class InOutTracker<TItem>
	{
		private List<TItem> _prevItems = new List<TItem>();
		private List<TItem> _curItems = new List<TItem>();
		private readonly List<TItem> _addedItems = new List<TItem>();
		private readonly List<TItem> _removedItems = new List<TItem>();
		private readonly HashSet<TItem> _itemSet = new HashSet<TItem>();

		private bool _updating = false;

		/// <summary>
		/// Returns true if there's no items in the tracker, indicating
		/// that no iteration is necessary.
		/// </summary>
		public bool Empty
		{
			get
			{
				if (!_updating)
					throw new InvalidOperationException("Call Begin() before checking if tracker is empty");

				return _prevItems.Count == 0 && _curItems.Count == 0;
			}
		}

		/// <summary>
		/// Returns the list that should be updated with the current
		/// items. It's not necessary to clear it.
		/// </summary>
		public IList<TItem> UpdateList
		{
			get
			{
				if (!_updating)
					throw new InvalidOperationException("Call Begin() before accessing update list");

				return _curItems;
			}
		}

		/// <summary>
		/// Returns the list of items that were added since the last
		/// update.
		/// </summary>
		public IReadOnlyList<TItem> Added
		{
			get
			{
				if (!_updating)
					throw new InvalidOperationException("Call Begin() before fetching added items");

				return _addedItems;
			}
		}

		/// <summary>
		/// Returns the list of items that were removed since the last
		/// update.
		/// </summary>
		public IReadOnlyList<TItem> Removed
		{
			get
			{
				if (!_updating)
					throw new InvalidOperationException("Call Begin() before fetching removed items");

				return _removedItems;
			}
		}

		/// <summary>
		/// Returns the list of items that are currently being tracked.
		/// </summary>
		/// <remarks>
		/// This list effectively represents the items that were found
		/// "inside" during the last update. For example, in the context
		/// of tracking visible actors, this list would represent the
		/// actors that were, and currently are, visible.
		/// </remarks>
		public IReadOnlyList<TItem> Current
		{
			get
			{
				if (_updating)
					throw new InvalidOperationException("Call End() before fetching current items");

				return _prevItems;
			}
		}

		/// <summary>
		/// Adds an item to the tracker outside of updates.
		/// </summary>
		/// <remarks>
		/// This method is intended for manual management of items that
		/// are currently "inside". For example, in the context of
		/// tracking visible actors, this method would be used to add an
		/// actor that became visible outside of the regular update cycle
		/// and should be considered part of the visible actors during the
		/// next update.
		/// </remarks>
		/// <param name="item"></param>
		/// <exception cref="InvalidOperationException"></exception>
		public void InjectItem(TItem item)
		{
			if (_updating)
				throw new InvalidOperationException("Call End() before adding items");

			_prevItems.Add(item);
		}

		/// <summary>
		/// Removes an item from the tracker outside of updates.
		/// </summary>
		/// <remarks>
		/// This method is intended for manual management of items that
		/// are currently "outside". For example, in the context of
		/// tracking visible actors, this method would be used to remove
		/// an actor that became non-visible outside of the regular update
		/// cycle and should be considered part of the non-visible actors
		/// during the next update.
		/// </remarks>
		/// <param name="item"></param>
		/// <exception cref="InvalidOperationException"></exception>
		public void EjectItem(TItem item)
		{
			if (_updating)
				throw new InvalidOperationException("Call End() before removing items");

			_prevItems.Remove(item);
		}

		/// <summary>
		/// Clears the tracker of all items.
		/// </summary>
		/// <remarks>
		/// Effectively resets the tracker to its initial state. The next
		/// update will consider all items that "inside" to be new items
		/// that were added with that update.
		/// </remarks>
		/// <exception cref="InvalidOperationException"></exception>
		public void ClearItems()
		{
			if (_updating)
				throw new InvalidOperationException("Call End() before clearing tracker");

			_prevItems.Clear();
			_curItems.Clear();
			_addedItems.Clear();
			_removedItems.Clear();
			_itemSet.Clear();
		}

		/// <summary>
		/// Starts the update, during which the tracker can be updated and
		/// read.
		/// </summary>
		/// <exception cref="InvalidOperationException"></exception>
		public void Begin()
		{
			if (_updating)
				throw new InvalidOperationException("End previous update using End() before starting new update");

			_updating = true;
		}

		/// <summary>
		/// Updates the list of added and removed items.
		/// </summary>
		/// <exception cref="InvalidOperationException"></exception>
		public void Update()
		{
			if (!_updating)
				throw new InvalidOperationException("Call Begin() before updating tracker");

			_itemSet.Clear();
			foreach (var area in _prevItems)
				_itemSet.Add(area);

			foreach (var item in _curItems)
			{
				if (_itemSet.Contains(item))
					continue;

				_addedItems.Add(item);
			}

			_itemSet.Clear();
			foreach (var area in _curItems)
				_itemSet.Add(area);

			foreach (var item in _prevItems)
			{
				if (_itemSet.Contains(item))
					continue;

				_removedItems.Add(item);
			}
		}

		/// <summary>
		/// Ends the update and prepares for the next one. This should be
		/// called after fetching the added and removed items.
		/// </summary>
		public void End()
		{
			if (!_updating)
				throw new InvalidOperationException("Call Begin() before ending update");

			_updating = false;

			var temp = _prevItems;
			_prevItems = _curItems;
			_curItems = temp;

			_addedItems.Clear();
			_removedItems.Clear();
			_curItems.Clear();
			_itemSet.Clear();
		}
	}
}
