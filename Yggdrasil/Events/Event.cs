using System;

namespace Yggdrasil.Events
{
	// Originally I wanted to use structs for events, so it wouldn't be necessary
	// to instantiate them, but that seemed like a hassle in the long run.
	// Still... this,
	// 
	//     public Event<EventArgs> Foobar;
	// 
	// does seem nicer than this.
	// 
	//     public Event<EventArgs> Foobar = new();

	/// <summary>
	/// A generic event that can be subscribed to and raised.
	/// </summary>
	/// <typeparam name="TArgs"></typeparam>
	public class Event<TArgs> where TArgs : EventArgs
	{
		private event EventHandler<TArgs> _event;

		/// <summary>
		/// Returns the owner of this event if any was set.
		/// </summary>
		/// <remarks>
		/// If no sender is specified when raising the event, the owner will be used.
		/// </remarks>
		public object Owner { get; }

		/// <summary>
		/// Returns whether this event has any subscribers.
		/// </summary>
		public bool HasSubscribers => _event != null;

		/// <summary>
		/// Creates a new event without owner.
		/// </summary>
		public Event()
		{
		}

		/// <summary>
		/// Creates a new event with the given owner.
		/// </summary>
		/// <param name="owner"></param>
		public Event(object owner)
		{
			this.Owner = owner;
		}

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
			this.Raise(this.Owner, args);
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
