﻿using System;

namespace Yggdrasil.Data.JSON.ObjectOriented
{
	/// <summary>
	/// Represents a database object that can be identified by an id.
	/// </summary>
	/// <typeparam name="TId"></typeparam>
	public interface IObjectData<TId> where TId : IEquatable<TId>
	{
		/// <summary>
		/// Returns the object's unique id.
		/// </summary>
		TId Id { get; set; }
	}

	/// <summary>
	/// Represents a database object that can be identified by an id and versioned.
	/// </summary>
	/// <typeparam name="TId"></typeparam>
	/// <typeparam name="TVersion"></typeparam>
	public interface IObjectData<TId, TVersion> : IObjectData<TId> where TId : IEquatable<TId> where TVersion : IComparable<TVersion>
	{
		/// <summary>
		/// Returns the object's version.
		/// </summary>
		TVersion Version { get; set; }
	}

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
}
