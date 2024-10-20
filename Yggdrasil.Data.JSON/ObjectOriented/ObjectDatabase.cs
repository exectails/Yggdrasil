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
	/// <typeparam name="TObject"></typeparam>
	public abstract class ObjectDatabase<TId, TObject> where TObject : IObjectData<TId>, new()
	{
		/// <summary>
		/// Returns the loaded entries.
		/// </summary>
		public Dictionary<TId, TObject> Entries { get; } = new Dictionary<TId, TObject>();

		/// <summary>
		/// Returns the warnings that were encountered while loading the database.
		/// </summary>
		public List<DatabaseWarningException> Warnings { get; } = new List<DatabaseWarningException>();

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
						if (!(token is JObject entry))
							continue;

						try
						{
							if (!entry.ContainsKey("id"))
								throw new MandatoryValueException(fileName, "id", entry);

							var id = this.ReadId(entry);
							var exists = this.Entries.TryGetValue(id, out var data);

							if (!exists)
							{
								if (this.MandatoryFields.Length > 0)
									entry.AssertNotMissing(this.MandatoryFields);

								data = new TObject { Id = id };
							}

							this.ReadEntry(entry, data);
							this.Entries[id] = data;
						}
						catch (MandatoryValueException ex)
						{
							this.Warnings.Add(new MandatoryValueException(fileName, ex.Key, entry));
							continue;
						}
						catch (DatabaseWarningException ex)
						{
							var msg = string.Format("{0}\n{1}", ex.Message, entry);

							this.Warnings.Add(new DatabaseWarningException(fileName, msg));
							continue;
						}
						catch (OverflowException)
						{
							var msg = string.Format("Number too big or too small for variable, in \n{0}", entry);

							this.Warnings.Add(new DatabaseWarningException(fileName, msg));
							continue;
						}
						catch (Exception ex)
						{
							var msg = string.Format("Exception: {0}\nEntry: \n{1}", ex, entry);

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
		/// Reads an entry's id from the database.
		/// </summary>
		/// <param name="entry"></param>
		/// <returns></returns>
		protected abstract TId ReadId(JObject entry);

		/// <summary>
		/// Reads an entry's data from the database into the object.
		/// </summary>
		/// <param name="entry"></param>
		/// <param name="data"></param>
		protected abstract void ReadEntry(JObject entry, TObject data);

		/// <summary>
		/// Called after the database has been loaded.
		/// </summary>
		protected virtual void AfterLoad()
		{
		}
	}
}
