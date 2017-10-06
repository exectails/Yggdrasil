// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yggdrasil.Data.JSON
{
	/// <summary>
	/// A text-based database using JSON.
	/// </summary>
	/// <typeparam name="TData"></typeparam>
	public abstract class DatabaseJson<TData> : Database<TData> where TData : class, new()
	{
		/// <summary>
		/// Loads data from given file.
		/// </summary>
		/// <param name="filePath"></param>
		public override void LoadFile(string filePath)
		{
			this.Warnings.Clear();

			using (var fs = new StreamReader(filePath))
			using (var reader = new JsonTextReader(fs))
			{
				try
				{
					var array = JArray.Load(reader);

					foreach (var entry in array)
					{
						var obj = entry as JObject;
						if (obj == null)
							continue;

						try
						{
							this.ReadEntry(obj);
						}
						catch (MandatoryValueException ex)
						{
							this.Warnings.Add(new MandatoryValueException(filePath, ex.Key, obj));
							continue;
						}
						catch (DatabaseWarningException ex)
						{
							var msg = string.Format("{0}\n{1}", ex.Message, obj);

							this.Warnings.Add(new DatabaseWarningException(filePath, msg));
							continue;
						}
						catch (OverflowException)
						{
							var msg = string.Format("Number too big or too small for variable, in \n{0}", obj);

							this.Warnings.Add(new DatabaseWarningException(filePath, msg));
							continue;
						}
						catch (Exception ex)
						{
							var msg = string.Format("Exception: {0}\nEntry: \n{1}", ex, obj);

							throw new DatabaseErrorException(filePath, msg);
						}
					}
				}
				catch (JsonReaderException ex)
				{
					// Throw to stop the server, databases depend on each
					// other, skipping one could lead to problems.
					throw new DatabaseErrorException(filePath, ex.Message);
				}
			}
		}

		/// <summary>
		/// Reads entry and adds information to database.
		/// </summary>
		/// <param name="entry"></param>
		protected abstract void ReadEntry(JObject entry);
	}

	/// <summary>
	/// A text-based database using JSON.
	/// </summary>
	/// <typeparam name="TData"></typeparam>
	public abstract class DatabaseJsonIndexed<TIndex, TData> : IndexedDatabase<TIndex, TData> where TData : class, new()
	{
		/// <summary>
		/// Loads data from given file.
		/// </summary>
		/// <param name="filePath"></param>
		public override void LoadFile(string filePath)
		{
			this.Warnings.Clear();

			using (var fs = new StreamReader(filePath))
			using (var reader = new JsonTextReader(fs))
			{
				try
				{
					var array = JArray.Load(reader);

					foreach (var entry in array)
					{
						var obj = entry as JObject;
						if (obj == null)
							continue;

						try
						{
							this.ReadEntry(obj);
						}
						catch (MandatoryValueException ex)
						{
							this.Warnings.Add(new MandatoryValueException(filePath, ex.Key, obj));
							continue;
						}
						catch (DatabaseWarningException ex)
						{
							var msg = string.Format("{0}\n{1}", ex.Message, obj);

							this.Warnings.Add(new DatabaseWarningException(filePath, msg));
							continue;
						}
						catch (OverflowException)
						{
							var msg = string.Format("Number too big or too small for variable, in \n{0}", obj);

							this.Warnings.Add(new DatabaseWarningException(filePath, msg));
							continue;
						}
						catch (Exception ex)
						{
							var msg = string.Format("Exception: {0}\nEntry: \n{1}", ex, obj);

							throw new DatabaseErrorException(filePath, msg);
						}
					}
				}
				catch (JsonReaderException ex)
				{
					// Throw to stop the server, databases depend on each
					// other, skipping one could lead to problems.
					throw new DatabaseErrorException(filePath, ex.Message);
				}
			}
		}

		/// <summary>
		/// Reads entry and adds information to database.
		/// </summary>
		/// <param name="entry"></param>
		protected abstract void ReadEntry(JObject entry);
	}
}
