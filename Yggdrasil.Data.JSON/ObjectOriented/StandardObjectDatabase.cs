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
	public abstract class StandardObjectDatabase<TObject> : DatabaseLoader, IDatabase where TObject : StandardObjectData, new()
	{
		int IDatabase.Count => this.Objects.Count;
		void IDatabase.Clear() => this.Objects.Clear();
		void IDatabase.LoadFile(string filePath) => this.Load(filePath);
		DatabaseWarningException[] IDatabase.GetWarnings() => this.Warnings.ToArray();

		/// <summary>
		/// Returns the target version of the database.
		/// </summary>
		public int Version { get; }

		/// <summary>
		/// Returns the loaded objects.
		/// </summary>
		public ObjectStorage<int, TObject> Objects { get; } = new ObjectStorage<int, TObject>();

		/// <summary>
		/// Creates a new object database.
		/// </summary>
		public StandardObjectDatabase() : this(default)
		{
		}

		/// <summary>
		/// Creates a new object database with the given target version.
		/// </summary>
		/// <param name="version"></param>
		public StandardObjectDatabase(int version)
		{
			this.Version = version;
		}

		/// <summary>
		/// Handles a single entry read from the database.
		/// </summary>
		/// <param name="jObj"></param>
		protected override void HandleEntry(JObject jObj)
		{
			var newObj = new TObject();
			this.ReadBaseData(jObj, newObj);

			if (this.Objects.TryGet(newObj.Id, out var existingObj))
			{
				if (this.ShouldOverride(newObj, existingObj))
				{
					this.ReadEntry(jObj, newObj, existingObj);
					this.InsertObject(newObj);
				}
			}
			else
			{
				this.CheckFields(jObj);
				this.ReadEntry(jObj, newObj, newObj);
				this.InsertObject(newObj);
			}
		}

		/// <summary>
		/// Reads the base data from the entry and sets it on the given object.
		/// </summary>
		/// <param name="entry"></param>
		/// <param name="dataObj"></param>
		protected void ReadBaseData(JObject entry, TObject dataObj)
		{
			entry.AssertNotMissing("id");

			dataObj.Id = entry.ReadInt("id");
			dataObj.Version = entry.ReadInt("version", 0);
		}

		/// <summary>
		/// Returns true if the new object should override the existing one.
		/// </summary>
		/// <remarks>
		/// Called for existing objects after the base data for the new object
		/// was read. The given new object does not represent a fully loaded
		/// object.
		/// </remarks>
		/// <param name="newObj"></param>
		/// <param name="existingObj"></param>
		/// <returns></returns>
		protected virtual bool ShouldOverride(TObject newObj, TObject existingObj)
		{
			var existingVersion = existingObj.Version;
			var newVersion = newObj.Version;

			return (newVersion.CompareTo(this.Version) <= 0 && newVersion.CompareTo(existingVersion) >= 0);
		}

		/// <summary>
		/// Returns true if the entry's fields are valid in number.
		/// </summary>
		/// <param name="entry"></param>
		/// <returns></returns>
		protected virtual void CheckFields(JObject entry)
		{
		}

		/// <summary>
		/// Reads an entry's data from the database into the object.
		/// </summary>
		/// <param name="entry"></param>
		/// <param name="dataObj"></param>
		/// <param name="existingObj"></param>
		protected abstract void ReadEntry(JObject entry, TObject dataObj, TObject existingObj);

		/// <summary>
		/// Inserts the object into the database, potentially replacing existing
		/// objects.
		/// </summary>
		/// <param name="obj"></param>
		protected virtual void InsertObject(TObject obj)
		{
			this.Objects.Insert(obj);
		}
	}
}
