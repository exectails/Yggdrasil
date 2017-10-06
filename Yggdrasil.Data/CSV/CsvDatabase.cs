// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using System;
using System.IO;
using System.Reflection;

namespace Yggdrasil.Data.CSV
{
	public abstract class CsvDatabase<TData> : Database<TData> where TData : class, new()
	{
		private int _min;

		/// <summary>
		/// Initializes database.
		/// </summary>
		protected CsvDatabase()
		{
			var method = this.GetType().GetMethod("ReadEntry", BindingFlags.NonPublic | BindingFlags.Instance);
			var attr = method.GetCustomAttributes(typeof(MinFieldCountAttribute), true);
			if (attr.Length > 0)
				_min = (attr[0] as MinFieldCountAttribute).Count;
		}

		/// <summary>
		/// Loads data from given CSV file.
		/// </summary>
		/// <param name="filePath"></param>
		public override void LoadFile(string filePath)
		{
			using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				var csv = new CsvReader(stream, filePath, ',');
				var fileName = Path.GetFileName(filePath).Replace("\\", "/");

				foreach (var entry in csv.Next())
				{
					var line = entry.Line;

					try
					{
						if (entry.Count < _min)
							throw new FieldCountException(_min, entry.Count);

						this.ReadEntry(entry);
					}
					catch (CsvDatabaseWarningException ex)
					{
						ex.Source = fileName;
						ex.Line = line;

						this.Warnings.Add(ex);
						continue;
					}
					catch (OverflowException)
					{
						var msg = string.Format("Variable not fit for type (#{0}).", entry.LastIndex);

						this.Warnings.Add(new CsvDatabaseWarningException(fileName, line, msg));
						continue;
					}
					catch (FormatException)
					{
						var msg = string.Format("Invalid number format (#{0}).", entry.LastIndex);

						this.Warnings.Add(new CsvDatabaseWarningException(fileName, line, msg));
						continue;
					}
					catch (ArgumentOutOfRangeException ex)
					{
						var msg = string.Format("Invalid value index at {0}", ex.StackTrace);

						this.Warnings.Add(new CsvDatabaseWarningException(fileName, line, msg));
						continue;
					}
				}
			}
		}

		/// <summary>
		/// Not available for CSV databases.
		/// </summary>
		/// <param name="stream"></param>
		public override void LoadStream(Stream stream)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Reads CSV entry and adds it to database.
		/// </summary>
		/// <param name="entry"></param>
		protected abstract void ReadEntry(CsvEntry entry);
	}

	/// <summary>
	/// Marks a CSV database to required at least the given amount amount
	/// of values in a line.
	/// </summary>
	public class MinFieldCountAttribute : Attribute
	{
		/// <summary>
		/// Number of required values.
		/// </summary>
		public int Count { get; protected set; }

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="count"></param>
		public MinFieldCountAttribute(int count)
		{
			this.Count = count;
		}
	}
}
