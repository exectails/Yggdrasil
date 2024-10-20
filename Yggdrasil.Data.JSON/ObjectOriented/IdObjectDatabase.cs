using Newtonsoft.Json.Linq;

namespace Yggdrasil.Data.JSON.ObjectOriented
{
	/// <summary>
	/// Loads and manages a set of database objects that can be identified by an
	/// integer id.
	/// </summary>
	/// <typeparam name="TObject"></typeparam>
	public abstract class IdObjectDatabase<TObject> : ObjectDatabase<int, TObject> where TObject : IObjectData<int>, new()
	{
		/// <summary>
		/// Reads an entry's id from the database.
		/// </summary>
		/// <param name="entry"></param>
		/// <returns></returns>
		protected override int ReadId(JObject entry)
			=> entry.ReadInt("id");
	}
}
