// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using System;
using System.IO;
using System.Reflection;

namespace Yggdrasil.Data.CSV
{
	/// <summary>
	/// A text-based database using CSV.
	/// </summary>
	/// <typeparam name="TData"></typeparam>
	public abstract class DatabaseCsv<TData> : Database<TData> where TData : class, new()
	{
		private int _min;

		/// <summary>
		/// Initializes database.
		/// </summary>
		protected DatabaseCsv()
		{
			var method = this.GetType().GetMethod("ReadEntry", BindingFlags.NonPublic | BindingFlags.Instance);
			var attr = method.GetCustomAttributes(typeof(MinFieldCountAttribute), true);
			if (attr.Length > 0)
				_min = (attr[0] as MinFieldCountAttribute).Count;
		}

		/// <summary>
		/// Loads data from given file.
		/// </summary>
		/// <param name="filePath"></param>
		public override void LoadFile(string filePath)
		{
			this.Warnings.Clear();

			using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				var csv = new CsvReader(stream, filePath, ',');
				var fileName = filePath.Replace("\\", "/");

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
					catch (IndexOutOfRange ex)
					{
						var msg = string.Format("Invalid index used at {0}", ex.StackTrace);

						this.Warnings.Add(new CsvDatabaseWarningException(fileName, line, msg));
						continue;
					}
					catch (Exception ex)
					{
						var msg = string.Format("Exception: {0}\nEntry: \n{1}", ex, line);

						throw new DatabaseErrorException(filePath, msg);
					}
				}
			}
		}

		/// <summary>
		/// Reads entry and adds information to database.
		/// </summary>
		/// <param name="entry"></param>
		protected abstract void ReadEntry(CsvEntry entry);
	}

	/// <summary>
	/// A text-based database using CSV.
	/// </summary>
	/// <typeparam name="TData"></typeparam>
	public abstract class DatabaseCsvIndexed<TIndex, TData> : IndexedDatabase<TIndex, TData> where TData : class, new()
	{
		private int _min;

		/// <summary>
		/// Initializes database.
		/// </summary>
		protected DatabaseCsvIndexed()
		{
			var method = this.GetType().GetMethod("ReadEntry", BindingFlags.NonPublic | BindingFlags.Instance);
			var attr = method.GetCustomAttributes(typeof(MinFieldCountAttribute), true);
			if (attr.Length > 0)
				_min = (attr[0] as MinFieldCountAttribute).Count;
		}

		/// <summary>
		/// Loads data from given file.
		/// </summary>
		/// <param name="filePath"></param>
		public override void LoadFile(string filePath)
		{
			this.Warnings.Clear();

			using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				var csv = new CsvReader(stream, filePath, ',');
				var fileName = filePath.Replace("\\", "/");

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
					catch (IndexOutOfRange ex)
					{
						var msg = string.Format("Invalid index used at {0}", ex.StackTrace);

						this.Warnings.Add(new CsvDatabaseWarningException(fileName, line, msg));
						continue;
					}
					catch (Exception ex)
					{
						var msg = string.Format("Exception: {0}\nEntry: \n{1}", ex, line);

						throw new DatabaseErrorException(filePath, msg);
					}
				}
			}
		}

		/// <summary>
		/// Reads entry and adds information to database.
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
