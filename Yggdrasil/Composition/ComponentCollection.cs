using System;
using System.Linq;
using System.Collections.Generic;
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
		private readonly object _syncLock = new object();

		private readonly Dictionary<Type, IComponent> _components = new Dictionary<Type, IComponent>();

		private readonly List<IUpdateable> _updateables = new List<IUpdateable>();
		private bool _updateablesDirty;

		/// <summary>
		/// Adds a component.
		/// </summary>
		/// <typeparam name="TComponent"></typeparam>
		/// <param name="component"></param>
		public void Add<TComponent>(TComponent component) where TComponent : IComponent
		{
			var type = component.GetType();
			lock (_syncLock)
			{
				_components[type] = component;

				if (component is IUpdateable)
					_updateablesDirty = true;
			}
		}

		/// <summary>
		/// Removes the given component, returns false if the
		/// component didn't exist.
		/// </summary>
		/// <param name="component"></param>
		public bool Remove(IComponent component)
		{
			var removed = false;

			var type = component.GetType();
			lock (_syncLock)
			{
				removed = _components.Remove(type);

				if (removed && component is IUpdateable)
					_updateablesDirty = true;
			}

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
			lock (_syncLock)
			{
				if (!_components.TryGetValue(typeof(TComponent), out var component))
					return false;

				return this.Remove(component);
			}
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

			lock (_syncLock)
			{
				if (_components.TryGetValue(type, out var component))
					return (TComponent)component;
			}

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
			lock (_syncLock)
				return _components.ContainsKey(typeof(TComponent));
		}

		/// <summary>
		/// Updates updatable components.
		/// </summary>
		/// <param name="elapsed"></param>
		public void Update(TimeSpan elapsed)
		{
			lock (_syncLock)
			{
				if (_updateablesDirty)
				{
					_updateables.Clear();
					_updateables.AddRange(_components.Values.OfType<IUpdateable>());
					_updateablesDirty = false;
				}
			}

			foreach (var component in _updateables)
				component.Update(elapsed);
		}
	}
}
