// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Yggdrasil.Extensions;

namespace Yggdrasil.Data.CSV
{
	/// <summary>
	/// Represents a line in a CSV file, allowing simple access its data.
	/// </summary>
	public class CsvEntry
	{
		/// <summary>
		/// The raw values in this entry.
		/// </summary>
		public List<string> Fields { get; }

		/// <summary>
		/// The line number of this entry.
		/// </summary>
		public int Line { get; }

		/// <summary>
		/// Returns the last index that was read.
		/// </summary>
		public int LastIndex { get; private set; }

		/// <summary>
		/// The amount of values in this entry.
		/// </summary>
		public int Count { get { return this.Fields.Count; } }

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="fields"></param>
		/// <param name="line"></param>
		public CsvEntry(List<string> fields, int line)
		{
			this.Fields = fields;
			this.Line = line;
		}

		/// <summary>
		/// Returns whether the value at the given index is null or
		/// white-space.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public bool IsFieldEmpty(int index)
		{
			return (this.Fields[index]).IsNullOrWhiteSpace();
		}

		/// <summary>
		/// Reads value from index and returns a value and the base to
		/// read it as via out.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="val"></param>
		/// <param name="fromBase"></param>
		private void GetValAndBase(int index, out string val, out int fromBase)
		{
			val = this.Fields[index];
			fromBase = 10;

			if (val.StartsWith("0x"))
			{
				val = val.Substring(2);
				fromBase = 16;
			}
			else if (val.StartsWith("0b"))
			{
				val = val.Substring(2);
				fromBase = 2;
			}
		}

		/// <summary>
		/// Returns value at given index as bool. Valid values for "true"
		/// are "1", "true", and "yes".
		/// </summary>
		/// <param name="index"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public bool ReadBool(int index, bool def = false)
		{
			if (index < 0 || index > this.Count - 1)
				throw new IndexOutOfRangeException();

			this.LastIndex = index;

			if (this.IsFieldEmpty(index))
				return def;

			var val = this.Fields[index];
			return (val == "1" || val == "true" || val == "yes");
		}

		/// <summary>
		/// Returns value at given index as byte.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public byte ReadByte(int index, byte def = 0)
		{
			if (index < 0 || index > this.Count - 1)
				throw new IndexOutOfRangeException();

			this.LastIndex = index;

			if (this.IsFieldEmpty(index))
				return def;

			this.GetValAndBase(index, out var val, out var fromBase);

			return Convert.ToByte(val, fromBase);
		}

		/// <summary>
		/// Returns value at given index as sbyte.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public sbyte ReadSByte(int index, sbyte def = 0)
		{
			if (index < 0 || index > this.Count - 1)
				throw new IndexOutOfRangeException();

			this.LastIndex = index;

			if (this.IsFieldEmpty(index))
				return def;

			this.GetValAndBase(index, out var val, out var fromBase);

			return Convert.ToSByte(val, fromBase);
		}

		/// <summary>
		/// Returns value at given index as short.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public short ReadShort(int index, short def = 0)
		{
			if (index < 0 || index > this.Count - 1)
				throw new IndexOutOfRangeException();

			this.LastIndex = index;

			if (this.IsFieldEmpty(index))
				return def;

			this.GetValAndBase(index, out var val, out var fromBase);

			return Convert.ToInt16(val, fromBase);
		}

		/// <summary>
		/// Returns value at given index as ushort.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public ushort ReadUShort(int index, ushort def = 0)
		{
			if (index < 0 || index > this.Count - 1)
				throw new IndexOutOfRangeException();

			this.LastIndex = index;

			if (this.IsFieldEmpty(index))
				return def;

			this.GetValAndBase(index, out var val, out var fromBase);

			return Convert.ToUInt16(val, fromBase);
		}

		/// <summary>
		/// Returns value at given index as int.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public int ReadInt(int index, int def = 0)
		{
			if (index < 0 || index > this.Count - 1)
				throw new IndexOutOfRangeException();

			this.LastIndex = index;

			if (this.IsFieldEmpty(index))
				return def;

			this.GetValAndBase(index, out var val, out var fromBase);

			return Convert.ToInt32(val, fromBase);
		}

		/// <summary>
		/// Returns value at given index as uint.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public uint ReadUInt(int index, uint def = 0)
		{
			if (index < 0 || index > this.Count - 1)
				throw new IndexOutOfRangeException();

			this.LastIndex = index;

			if (this.IsFieldEmpty(index))
				return def;

			this.GetValAndBase(index, out var val, out var fromBase);

			return Convert.ToUInt32(val, fromBase);
		}

		/// <summary>
		/// Returns value at given index as long.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public long ReadLong(int index, long def = 0)
		{
			if (index < 0 || index > this.Count - 1)
				throw new IndexOutOfRangeException();

			this.LastIndex = index;

			if (this.IsFieldEmpty(index))
				return def;

			this.GetValAndBase(index, out var val, out var fromBase);

			return Convert.ToInt64(val, fromBase);
		}

		/// <summary>
		/// Returns value at given index as ulong.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public ulong ReadULong(int index, ulong def = 0)
		{
			if (index < 0 || index > this.Count - 1)
				throw new IndexOutOfRangeException();

			this.LastIndex = index;

			if (this.IsFieldEmpty(index))
				return def;

			this.GetValAndBase(index, out var val, out var fromBase);

			return Convert.ToUInt64(val, fromBase);
		}

		/// <summary>
		/// Returns value at given index as float.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public float ReadFloat(int index, float def = 0)
		{
			if (index < 0 || index > this.Count - 1)
				throw new IndexOutOfRangeException();

			this.LastIndex = index;

			if (this.IsFieldEmpty(index))
				return def;

			return Convert.ToSingle(this.Fields[index], CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Returns value at given index as double.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public double ReadDouble(int index, double def = 0)
		{
			if (index < 0 || index > this.Count - 1)
				throw new IndexOutOfRangeException();

			this.LastIndex = index;

			if (this.IsFieldEmpty(index))
				return def;

			return Convert.ToDouble(this.Fields[index], CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Returns value at given index as string.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public string ReadString(int index, string def = null)
		{
			if (index < 0 || index > this.Count - 1)
				throw new IndexOutOfRangeException();

			this.LastIndex = index;

			if (this.IsFieldEmpty(index))
				return def;

			return (this.Fields[index]);
		}

		/// <summary>
		/// Returns values at given index as list of strings, seperated by
		/// colons (:).
		/// </summary>
		/// <param name="index"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public string[] ReadStringList(int index, string[] def = null)
		{
			if (index < 0 || index > this.Count - 1)
				throw new IndexOutOfRangeException();

			this.LastIndex = index;

			if (this.IsFieldEmpty(index))
				return def;

			return this.ReadString(index).Split(':');
		}

		/// <summary>
		/// Returns values at given index as list of strings, seperated by
		/// colons (:).
		/// </summary>
		/// <param name="index"></param>
		/// <param name="def"></param>
		/// <returns></returns>
		public int[] ReadIntList(int index, int[] def = null)
		{
			if (index < 0 || index > this.Count - 1)
				throw new IndexOutOfRangeException();

			this.LastIndex = index;

			if (this.IsFieldEmpty(index))
				return def;

			var valuesSplit = this.ReadString(index).Split(':');
			var values = valuesSplit.Select(a => Convert.ToInt32(a));

			return values.ToArray();
		}
	}
}
