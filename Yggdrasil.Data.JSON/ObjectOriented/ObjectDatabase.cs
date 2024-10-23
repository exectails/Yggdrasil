using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yggdrasil.Data.JSON.ObjectOriented
{
	/// <summary>
	/// Loads and manages a set of database objects.
	/// </summary>
	/// <typeparam name="TId"></typeparam>
	/// <typeparam name="TVersion"></typeparam>
	/// <typeparam name="TObject"></typeparam>
	public abstract class ObjectDatabase<TId, TVersion, TObject> : IDatabase
		where TId : IEquatable<TId>
		where TVersion : IComparable<TVersion>
		where TObject : IObjectData<TId, TVersion>, new()
	{
		int IDatabase.Count => this.Objects.Count;
		void IDatabase.Clear() => this.Objects.Clear();
		void IDatabase.LoadFile(string filePath) => this.Load(filePath);
		DatabaseWarningException[] IDatabase.GetWarnings() => this.Warnings.ToArray();

		/// <summary>
		/// Returns the target version of the database.
		/// </summary>
		public TVersion Version { get; }

		/// <summary>
		/// Returns the loaded objects.
		/// </summary>
		public ObjectStorage<TId, TObject> Objects { get; } = new ObjectStorage<TId, TObject>();

		/// <summary>
		/// Returns the warnings that were encountered during the last call to
		/// Load.
		/// </summary>
		public List<DatabaseWarningException> Warnings { get; } = new List<DatabaseWarningException>();

		/// <summary>
		/// Creates a new object database.
		/// </summary>
		public ObjectDatabase() : this(default)
		{
		}

		/// <summary>
		/// Creates a new object database with the given target version.
		/// </summary>
		/// <param name="version"></param>
		public ObjectDatabase(TVersion version)
		{
			this.Version = version;
		}

		/// <summary>
		/// Loads the database from the given file path.
		/// </summary>
		/// <param name="filePath"></param>
		public void Load(string filePath)
		{
			using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
				this.Load(filePath, fs);
		}

		/// <summary>
		/// Loads the database from the given stream.
		/// </summary>
		/// <param name="fileName">File name to use as reference in case of errors.</param>
		/// <param name="stream">Stream to load data from.</param>
		public void Load(string fileName, Stream stream)
		{
			this.Warnings.Clear();

			using (var sr = new StreamReader(stream))
			using (var reader = new JsonTextReader(sr))
			{
				try
				{
					var array = JArray.Load(reader);

					foreach (var token in array)
					{
						if (!(token is JObject jObj))
							continue;

						try
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
						catch (MandatoryValueException ex)
						{
							this.Warnings.Add(new MandatoryValueException(fileName, ex.Key, jObj));
							continue;
						}
						catch (DatabaseWarningException ex)
						{
							var msg = string.Format("{0}\n{1}", ex.Message, jObj);

							this.Warnings.Add(new DatabaseWarningException(fileName, msg));
							continue;
						}
						catch (OverflowException)
						{
							var msg = string.Format("Number too big or too small for variable, in \n{0}", jObj);

							this.Warnings.Add(new DatabaseWarningException(fileName, msg));
							continue;
						}
						catch (Exception ex)
						{
							var msg = string.Format("Exception: {0}\nEntry: \n{1}", ex, jObj);

							throw new DatabaseErrorException(fileName, msg);
						}
					}
				}
				catch (JsonReaderException ex)
				{
					// Throw to stop the server, databases depend on each
					// other, skipping one could lead to problems.
					throw new DatabaseErrorException(fileName, ex.Message);
				}
			}

			this.AfterLoad();
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
		/// Reads the basic data from the entry necessary to handle the object.
		/// </summary>
		/// <param name="entry"></param>
		/// <param name="dataObj"></param>
		protected abstract void ReadBaseData(JObject entry, TObject dataObj);

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

		/// <summary>
		/// Called after the database has been loaded.
		/// </summary>
		protected virtual void AfterLoad()
		{
		}
	}
}
