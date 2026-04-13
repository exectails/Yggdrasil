using System;

namespace Yggdrasil.Events
{
	/// <summary>
	/// A generic event that can be subscribed to and raised.
	/// </summary>
	/// <remarks>
	/// Note that due to this being a struct, it must be used as a
	/// non-readonly field, or it might not work as intended. If the
	/// instance is copied during access, (un)subscriptions will not
	/// affect the original instance.
	/// </remarks>
	/// <typeparam name="TArgs"></typeparam>
	public struct Event<TArgs> where TArgs : EventArgs
	{
		private event EventHandler<TArgs> _event;

		/// <summary>
		/// Returns whether this event has any subscribers.
		/// </summary>
		public bool HasSubscribers => _event != null;

		/// <summary>
		/// Subscribes the handler to the event.
		/// </summary>
		/// <param name="handler"></param>
		public void Subscribe(EventHandler<TArgs> handler)
		{
			_event += handler;
		}

		/// <summary>
		/// Unsubscribes the handler from the event.
		/// </summary>
		/// <param name="handler"></param>
		public void Unsubscribe(EventHandler<TArgs> handler)
		{
			_event -= handler;
		}

		/// <summary>
		/// Raises the event.
		/// </summary>
		/// <param name="args"></param>
		public void Raise(TArgs args)
		{
			this.Raise(null, args);
		}

		/// <summary>
		/// Raises the event.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public void Raise(object sender, TArgs args)
		{
			_event?.Invoke(sender, args);
		}
	}
}
