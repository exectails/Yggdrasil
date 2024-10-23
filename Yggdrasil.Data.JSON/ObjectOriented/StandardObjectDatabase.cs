using Newtonsoft.Json.Linq;

namespace Yggdrasil.Data.JSON.ObjectOriented
{
	/// <summary>
	/// Represents a database object that can be identified by an integer id
	/// and versioned by a numeric version.
	/// </summary>
	public abstract class StandardObjectData : IObjectData<int, int>
	{
		/// <summary>
		/// Returns the object's unique id.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Returns the object's version.
		/// </summary>
		public int Version { get; set; }
	}

	/// <summary>
	/// Loads and manages a set of database objects that can be identified by an
	/// integer id and determines overrideability by a numeric version.
	/// </summary>
	/// <typeparam name="TObject"></typeparam>
	public abstract class StandardObjectDatabase<TObject> : ObjectDatabase<int, int, TObject> where TObject : StandardObjectData, new()
	{
		/// <summary>
		/// Reads the base data from the entry and sets it on the given object.
		/// </summary>
		/// <param name="entry"></param>
		/// <param name="dataObj"></param>
		protected override void ReadBaseData(JObject entry, TObject dataObj)
		{
			entry.AssertNotMissing("id");

			dataObj.Id = entry.ReadInt("id");
			dataObj.Version = entry.ReadInt("version", 0);
		}
	}
}
