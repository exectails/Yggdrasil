using Yggdrasil.Scheduling;

namespace Yggdrasil.Composition
{
	/// <summary>
	/// Base interface for components.
	/// </summary>
	public interface IComponent
	{
	}

	/// <summary>
	/// Base interface for updatable components.
	/// </summary>
	public interface IUpdatableComponent : IComponent, IUpdateable
	{
	}
}
