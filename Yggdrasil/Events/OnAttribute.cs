using System;
using System.Linq;
using System.Reflection;

namespace Yggdrasil.Events
{
	/// <summary>
	/// Attribute for methods, to mark them as subscribers for events in an
	/// event manager.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class OnAttribute : Attribute
	{
		/// <summary>
		/// Returns the names of the events to subscribe to.
		/// </summary>
		public string[] EventNames { get; protected set; }

		/// <summary>
		/// Creates new attribute.
		/// </summary>
		/// <param name="eventNames"></param>
		public OnAttribute(params string[] eventNames)
		{
			if (eventNames == null || eventNames.Length == 0)
				throw new ArgumentException("At least one event name must be provided.");

			this.EventNames = eventNames;
		}

		/// <summary>
		/// Loads and subscribes all methods with the On attribute in the
		/// given subscriber object to the respective events in the event
		/// manager.
		/// </summary>
		/// <remarks>
		/// Uses reflection to find all event handlers in the subscriber
		/// and the events in the manager that match them based on the
		/// names defined in their OnAttributes.
		/// </remarks>
		/// <param name="subscriber"></param>
		/// <param name="eventManager"></param>
		public static void Load(object subscriber, object eventManager)
		{
			var subscriberType = subscriber.GetType();

			var methods = subscriberType.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			foreach (var method in methods)
			{
				var attrs = method.GetCustomAttributes<OnAttribute>(false);
				if (!attrs.Any())
					continue;

				foreach (var eventName in attrs.SelectMany(a => a.EventNames))
				{
					try
					{
						SubscribeToEvent(subscriber, method, eventManager, eventName);
					}
					catch (Exception ex)
					{
						throw new SubscriptionException(subscriberType.Name, method.Name, eventName, ex);
					}
				}
			}
		}

		/// <summary>
		/// Unsubscribes all methods with the On attribute in the subscriber
		/// from the respective events in the event manager.
		/// </summary>
		/// <param name="subscriber"></param>
		/// <param name="eventManager"></param>
		public static void Unload(object subscriber, object eventManager)
		{
			var subscriberType = subscriber.GetType();

			var methods = subscriberType.GetMethods();
			foreach (var method in methods)
			{
				var attrs = method.GetCustomAttributes<OnAttribute>(false);
				if (!attrs.Any())
					continue;

				foreach (var eventName in attrs.SelectMany(a => a.EventNames))
				{
					try
					{
						UnsubscribeFromEvent(subscriber, method, eventManager, eventName);
					}
					catch (Exception ex)
					{
						throw new SubscriptionException(subscriberType.Name, method.Name, eventName, ex);
					}
				}
			}
		}

		/// <summary>
		/// Subscribes the subscriber method to the event with the given name on
		/// the event manager. Throws if the event does not exist or the method
		/// signature doesn't match.
		/// </summary>
		/// <param name="subscriber"></param>
		/// <param name="method"></param>
		/// <param name="eventManager"></param>
		/// <param name="eventName"></param>
		private static void SubscribeToEvent(object subscriber, MethodInfo method, object eventManager, string eventName)
		{
			var eventManagerType = eventManager.GetType();

			var field = eventManagerType.GetField(eventName);
			if (field != null && field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(Event<>))
			{
				var eventVal = field.GetValue(eventManager);
				var subscribeMethod = eventVal.GetType().GetMethod("Subscribe");

				var paramType = method.GetParameters()[1].ParameterType;
				var callback = Delegate.CreateDelegate(typeof(EventHandler<>).MakeGenericType(paramType), subscriber, method);

				subscribeMethod.Invoke(eventVal, new object[] { callback });

				// Reassign the field in case it's a struct and we were subscribing
				// to a copy of the original.
				field.SetValue(eventManager, eventVal);
			}
			else
			{
				var eventHandlerInfo = eventManagerType.GetEvent(eventName) ?? throw new ArgumentException($"Unknown event '{eventName}'.");
				eventHandlerInfo.AddEventHandler(eventManager, Delegate.CreateDelegate(eventHandlerInfo.EventHandlerType, subscriber, method));
			}
		}

		/// <summary>
		/// Unsubscribes the subscriber method from the event with the given name
		/// on the event manager. Throws if the event does not exist or the method
		/// signature doesn't match.
		/// </summary>
		/// <param name="subscriber"></param>
		/// <param name="method"></param>
		/// <param name="eventManager"></param>
		/// <param name="eventName"></param>
		private static void UnsubscribeFromEvent(object subscriber, MethodInfo method, object eventManager, string eventName)
		{
			var eventManagerType = eventManager.GetType();

			var field = eventManagerType.GetField(eventName);
			if (field != null && field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(Event<>))
			{
				var eventVal = field.GetValue(eventManager);
				var unsubscribeMethod = eventVal.GetType().GetMethod("Unsubscribe");

				var paramType = method.GetParameters()[1].ParameterType;
				var callback = Delegate.CreateDelegate(typeof(EventHandler<>).MakeGenericType(paramType), subscriber, method);

				unsubscribeMethod.Invoke(eventVal, new object[] { callback });

				field.SetValue(eventManager, eventVal);
			}
			else
			{
				var eventHandlerInfo = eventManagerType.GetEvent(eventName) ?? throw new ArgumentException($"Unknown event '{eventName}'.");
				eventHandlerInfo.RemoveEventHandler(eventManager, Delegate.CreateDelegate(eventHandlerInfo.EventHandlerType, subscriber, method));
			}
		}
	}

	/// <summary>
	/// Exception thrown when a (un)subscription via the On attribute fails.
	/// </summary>
	public class SubscriptionException : Exception
	{
		/// <summary>
		/// Returns the name of the subscriber.
		/// </summary>
		public string SubscriberName { get; }

		/// <summary>
		/// Returns the name of the method that failed to subscribe.
		/// </summary>
		public string MethodName { get; }

		/// <summary>
		/// Returns the name of the event that failed to subscribe to.
		/// </summary>
		public string EventName { get; }

		/// <summary>
		/// Creates new exception.
		/// </summary>
		/// <param name="subscriberName"></param>
		/// <param name="methodName"></param>
		/// <param name="eventName"></param>
		/// <param name="innerException"></param>
		public SubscriptionException(string subscriberName, string methodName, string eventName, Exception innerException)
			: base($"Failed to update subscription of '{subscriberName}.{methodName}' to '{eventName}': {innerException.Message}", innerException)
		{
			this.SubscriberName = subscriberName;
			this.MethodName = methodName;
			this.EventName = eventName;
		}
	}
}
