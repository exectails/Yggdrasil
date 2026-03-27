using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Yggdrasil.Scheduling;

namespace Yggdrasil.Composition
{
	/// <summary>
	/// A collection of components for an entity.
	/// </summary>
	/// <remarks>
	/// Components are addressed by their type, and there can only be
	/// one component per type. For example, only one inventory, one
	/// skill collection, etc.
	/// </remarks>
	public class ComponentCollection : IUpdateable
	{
		private readonly ConcurrentDictionary<Type, IComponent> _components = new ConcurrentDictionary<Type, IComponent>();

		private volatile IUpdateable[] _updateables = new IUpdateable[0];
		private int _updateablesDirty;

		/// <summary>
		/// Adds a component.
		/// </summary>
		/// <typeparam name="TComponent"></typeparam>
		/// <param name="component"></param>
		public void Add<TComponent>(TComponent component) where TComponent : IComponent
		{
			var type = component.GetType();
			_components[type] = component;

			if (component is IUpdateable)
				Interlocked.Exchange(ref _updateablesDirty, 1);
		}

		/// <summary>
		/// Removes the given component, returns false if the
		/// component didn't exist.
		/// </summary>
		/// <param name="component"></param>
		public bool Remove(IComponent component)
		{
			var type = component.GetType();
			var removed = _components.TryRemove(type, out _);

			if (removed && component is IUpdateable)
				Interlocked.Exchange(ref _updateablesDirty, 1);

			return removed;
		}

		/// <summary>
		/// Removes component of the given type, returns false if the
		/// component didn't exist.
		/// </summary>
		/// <typeparam name="TComponent"></typeparam>
		/// <returns></returns>
		public bool Remove<TComponent>()
		{
			var type = typeof(TComponent);
			var removed = _components.TryRemove(type, out var component);

			if (removed && component is IUpdateable)
				Interlocked.Exchange(ref _updateablesDirty, 1);

			return removed;
		}

		/// <summary>
		/// Removes all components.
		/// </summary>
		public void Clear()
		{
			_components.Clear();
			Interlocked.Exchange(ref _updateablesDirty, 1);
		}

		/// <summary>
		/// Returns the component of the given type, or the type's default
		/// value if the component wasn't found.
		/// </summary>
		/// <typeparam name="TComponent"></typeparam>
		/// <returns></returns>
		public TComponent Get<TComponent>()
		{
			var type = typeof(TComponent);

			if (_components.TryGetValue(type, out var component))
				return (TComponent)component;

			return default;
		}

		/// <summary>
		/// Returns the component of the given type via out, returns false
		/// if the component wasn't found.
		/// </summary>
		/// <typeparam name="TComponent"></typeparam>
		/// <param name="component"></param>
		/// <returns></returns>
		public bool TryGet<TComponent>(out TComponent component)
		{
			component = this.Get<TComponent>();
			return component != null;
		}

		/// <summary>
		/// Returns true if a component with the given type exists.
		/// </summary>
		/// <typeparam name="TComponent"></typeparam>
		/// <returns></returns>
		public bool Has<TComponent>()
		{
			return _components.ContainsKey(typeof(TComponent));
		}

		/// <summary>
		/// Updates updatable components.
		/// </summary>
		/// <param name="elapsed"></param>
		public void Update(TimeSpan elapsed)
		{
			if (Interlocked.CompareExchange(ref _updateablesDirty, 0, 1) == 1)
				_updateables = _components.Values.OfType<IUpdateable>().ToArray();

			var updateables = _updateables;

			foreach (var component in updateables)
				component.Update(elapsed);
		}
	}
}
