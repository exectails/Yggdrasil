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
		TId Id { get; set; }
	}

	/// <summary>
	/// Represents a database object that can be identified by an integer id.
	/// </summary>
	public abstract class IdObjectData : IObjectData<int>
	{
		/// <summary>
		/// Returns the object's unique id.
		/// </summary>
		public int Id { get; set; }
	}
}
