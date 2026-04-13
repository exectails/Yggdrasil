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
