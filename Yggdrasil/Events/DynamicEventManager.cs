using System;
using System.Collections.Generic;

namespace Yggdrasil.Events
{
	/// <summary>
	/// An event manager that allows arbitrary events to be registered or raised.
	/// </summary>
	public class DynamicEventManager
	{
		private readonly Dictionary<string, EventHandler> _events = new Dictionary<string, EventHandler>();

		/// <summary>
		/// Subscribes to the event with the given name.
		/// </summary>
		/// <param name="eventName"></param>
		/// <param name="handler"></param>
		public void Subscribe(string eventName, EventHandler handler)
		{
			lock (_events)
			{
				if (!_events.ContainsKey(eventName))
					_events[eventName] = null;

				_events[eventName] += handler;
			}
		}

		/// <summary>
		/// Unsubscribes from the event with the given name.
		/// </summary>
		/// <param name="eventName"></param>
		/// <param name="handler"></param>
		public void Unsubscribe(string eventName, EventHandler handler)
		{
			lock (_events)
			{
				if (!_events.ContainsKey(eventName))
					return;

				var newValue = (_events[eventName] -= handler);

				if (newValue == null)
					_events.Remove(eventName);
			}
		}

		/// <summary>
		/// Raises the event with the given name.
		/// </summary>
		/// <param name="eventName"></param>
		/// <param name="args"></param>
		public void Raise(string eventName, EventArgs args)
			=> this.Raise(eventName, null, args);

		/// <summary>
		/// Raises the event with the given name.
		/// </summary>
		/// <param name="eventName"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public void Raise(string eventName, object sender, EventArgs args)
		{
			EventHandler handler = null;

			lock (_events)
			{
				if (_events.TryGetValue(eventName, out var h))
					handler = h;
			}

			handler?.Invoke(sender, args);
		}
	}
}
