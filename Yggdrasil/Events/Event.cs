using System;

namespace Yggdrasil.Events
{
	/// Using a struct for this is kind of dumb, because you have to jump through
	/// hoops to handle it generically, but it's nice to not have to instantiate
	/// the event object. Worth it? Maybe.

	/// <summary>
	/// A generic event that can be subscribed to and raised.
	/// </summary>
	/// <typeparam name="TArgs"></typeparam>
	public struct Event<TArgs> where TArgs : EventArgs
	{
		private event EventHandler<TArgs> _event;

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
