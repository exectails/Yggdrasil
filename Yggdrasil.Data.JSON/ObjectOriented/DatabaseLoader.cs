using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yggdrasil.Data.JSON.ObjectOriented
{
	/// <summary>
	/// Loads database entries from a file or stream.
	/// </summary>
	public abstract class DatabaseLoader
	{
		/// <summary>
		/// Returns the warnings that were encountered during the last call to
		/// Load.
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
		/// Loads the database entries from the given stream.
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
							this.HandleEntry(jObj);
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
		/// Handles a single entry read from the database.
		/// </summary>
		/// <param name="jObj"></param>
		protected abstract void HandleEntry(JObject jObj);

		/// <summary>
		/// Called after all entries were loaded and handled.
		/// </summary>
		protected virtual void AfterLoad()
		{
		}
	}
}
