namespace Yggdrasil.Data.JSON.ObjectOriented
{
	/// <summary>
	/// Represents a database object that can be identified by an id.
	/// </summary>
	/// <typeparam name="TId"></typeparam>
	public interface IObjectData<TId>
	{
		/// <summary>
		/// Returns the object's unique id.
		/// </summary>
		TId Id { get; }
	}
}
