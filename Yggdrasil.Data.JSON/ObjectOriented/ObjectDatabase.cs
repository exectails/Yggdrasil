using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yggdrasil.Data.JSON.ObjectOriented
{
	/// <summary>
	/// Loads and manages a set of database objects.
	/// </summary>
	/// <typeparam name="TId"></typeparam>
	/// <typeparam name="TObject"></typeparam>
	public abstract class ObjectDatabase<TId, TObject> : IDatabase where TObject : IObjectData<TId>, new()
	{
		void IDatabase.LoadFile(string filePath) => this.Load(filePath);
		DatabaseWarningException[] IDatabase.GetWarnings() => this.Warnings.ToArray();

		/// <summary>
		/// Returns the loaded objects.
		/// </summary>
		public Dictionary<TId, TObject> Objects { get; } = new Dictionary<TId, TObject>();

		/// <summary>
		/// Returns the warnings that were encountered during the last call to
		/// Load.
		/// </summary>
		public List<DatabaseWarningException> Warnings { get; } = new List<DatabaseWarningException>();

		/// <summary>
		/// Returns the number of loaded objects.
		/// </summary>
		public int Count => this.Objects.Count;

		/// <summary>
		/// Removes all entries from the database.
		/// </summary>
		public void Clear()
		{
			this.Objects.Clear();
			this.Warnings.Clear();
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
							if (!jObj.ContainsKey("id"))
								throw new MandatoryValueException(fileName, "id", jObj);

							var id = this.ReadId(jObj);
							var exists = this.Objects.TryGetValue(id, out var dataObj);

							if (!exists)
							{
								if (this.MandatoryFields.Length > 0)
									jObj.AssertNotMissing(this.MandatoryFields);

								dataObj = new TObject { Id = id };
							}
							else if (!this.ShouldOverride(jObj, dataObj))
							{
								continue;
							}

							this.ReadEntry(jObj, dataObj);
							this.AddOrReplace(dataObj);
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
		/// Returns a list of mandatory fields that must be present on the
		/// first version of an object.
		/// </summary>
		protected virtual string[] MandatoryFields { get; } = new string[0];

		/// <summary>
		/// Reads the id from a database entry.
		/// </summary>
		/// <param name="entry"></param>
		/// <returns></returns>
		protected abstract TId ReadId(JObject entry);

		/// <summary>
		/// Returns true if the given entry should override data in an existing
		/// object.
		/// </summary>
		/// <param name="entry"></param>
		/// <param name="dataObj"></param>
		/// <returns></returns>
		protected virtual bool ShouldOverride(JObject entry, TObject dataObj)
		{
			return true;
		}

		/// <summary>
		/// Reads an entry's data from the database into the object.
		/// </summary>
		/// <param name="entry"></param>
		/// <param name="dataObj"></param>
		protected abstract void ReadEntry(JObject entry, TObject dataObj);

		/// <summary>
		/// Called after the database has been loaded.
		/// </summary>
		protected virtual void AfterLoad()
		{
		}

		/// <summary>
		/// Adds the given object to the database, replacing any already existing
		/// entries.
		/// </summary>
		/// <param name="dataObj"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public void AddOrReplace(TObject dataObj)
		{
			if (dataObj == null)
				throw new ArgumentNullException(nameof(dataObj));

			this.Objects[dataObj.Id] = dataObj;
		}

		/// <summary>
		/// Returns true if the database contains an object with the given id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public bool Contains(TId id)
			=> this.Objects.ContainsKey(id);

		/// <summary>
		/// Returns the object with the given id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException">
		/// Thrown if the object with the given id does not exist.
		/// </exception>
		public TObject Get(TId id)
		{
			if (!this.Objects.TryGetValue(id, out var dataObj))
				throw new ArgumentException("Object not found.", nameof(id));

			return dataObj;
		}

		/// <summary>
		/// Returns the object with the given id via out. Returns false if no
		/// matching object was found.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public bool TryGet(TId id, out TObject data)
		{
			if (this.Objects.TryGetValue(id, out data))
				return true;

			data = default;
			return false;
		}

		/// <summary>
		/// Returns the first object that matches the given predicate.
		/// Returns null if no matching object was found.
		/// </summary>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public TObject Find(Func<TObject, bool> predicate)
			=> this.Objects.Values.FirstOrDefault(predicate);

		/// <summary>
		/// Returns all objects that match the given predicate.
		/// </summary>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public TObject[] FindAll(Func<TObject, bool> predicate)
			=> this.Objects.Values.Where(predicate).ToArray();
	}
}
