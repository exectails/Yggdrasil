using System;
using System.Collections.Generic;
using System.Linq;

namespace Yggdrasil.Data.JSON.ObjectOriented
{
	/// <summary>
	/// Manages a collection of database objects and grants access to them.
	/// </summary>
	/// <typeparam name="TId"></typeparam>
	/// <typeparam name="TObject"></typeparam>
	public class ObjectStorage<TId, TObject>
		where TId : IEquatable<TId>
		where TObject : IObjectData<TId>
	{
		private readonly Dictionary<TId, TObject> _objects = new Dictionary<TId, TObject>();

		/// <summary>
		/// Returns the number of objects in the storage.
		/// </summary>
		public int Count => _objects.Count;

		/// <summary>
		/// Adds the given object to the storage, replacing any already existing
		/// entries.
		/// </summary>
		/// <param name="dataObj"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public void AddOrReplace(TObject dataObj)
		{
			if (dataObj == null)
				throw new ArgumentNullException(nameof(dataObj));

			_objects[dataObj.Id] = dataObj;
		}

		/// <summary>
		/// Returns true if the storage contains an object with the given id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public bool Contains(TId id)
			=> _objects.ContainsKey(id);

		/// <summary>
		/// Removes all objects.
		/// </summary>
		public void Clear()
			=> _objects.Clear();

		/// <summary>
		/// Returns a list of all loaded objects.
		/// </summary>
		/// <returns></returns>
		public TObject[] GetAll()
			=> _objects.Values.ToArray();

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
			if (!_objects.TryGetValue(id, out var dataObj))
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
			if (_objects.TryGetValue(id, out data))
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
			=> _objects.Values.FirstOrDefault(predicate);

		/// <summary>
		/// Returns all objects that match the given predicate.
		/// </summary>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public TObject[] FindAll(Func<TObject, bool> predicate)
			=> _objects.Values.Where(predicate).ToArray();
	}
}
