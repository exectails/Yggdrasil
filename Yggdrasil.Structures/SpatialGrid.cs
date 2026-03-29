using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Yggdrasil.Geometry;

namespace Yggdrasil.Structures
{
	/// <summary>
	/// Describes an object that can be put into a spatial grid.
	/// </summary>
	public interface ISpatialObject
	{
		/// <summary>
		/// Returns the bounding box of the object.
		/// </summary>
		BoundingBoxF Bounds { get; }
	}

	/// <summary>
	/// A spacial grid structure for managing objects in a 2D space.
	/// </summary>
	/// <typeparam name="TObject"></typeparam>
	public class SpatialGrid<TObject> where TObject : class, ISpatialObject
	{
		private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

		private readonly int _cellSize;

		private readonly Dictionary<Coordinates, HashSet<TObject>> _cells = new Dictionary<Coordinates, HashSet<TObject>>();
		private readonly Dictionary<TObject, CellBounds> _objectCells = new Dictionary<TObject, CellBounds>();

		private readonly List<TObject> _updateObjects = new List<TObject>();
		private readonly ConcurrentBag<HashSet<TObject>> _hashSetPool = new ConcurrentBag<HashSet<TObject>>();

		/// <summary>
		/// Returns the number of objects currently in the spatial grid.
		/// </summary>
		public int ObjectCount { get; private set; }

		/// <summary>
		/// Returns the threshold for pooling objects.
		/// </summary>
		private int PoolObjectThreshold => Math.Max(100, this.ObjectCount / 4);

		/// <summary>
		/// Returns a list of all cell coordinates currently in the
		/// spatial grid.
		/// </summary>
		public List<Vector2> Cells
		{
			get
			{
				_lock.EnterWriteLock();
				try
				{
					return new List<Vector2>(_cells.Keys.Select(a => new Vector2(a.X, a.Y)));
				}
				finally
				{
					_lock.ExitWriteLock();
				}
			}
		}

		/// <summary>
		/// Creates a new spatial grid with the specified cell size.
		/// </summary>
		/// <param name="cellSize"></param>
		/// <exception cref="ArgumentException"></exception>
		public SpatialGrid(int cellSize)
		{
			if (cellSize <= 0)
				throw new ArgumentException("Cell size must be greater than zero.");

			_cellSize = cellSize;
		}

		/// <summary>
		/// Adds object to the spatial grid.
		/// </summary>
		/// <param name="obj"></param>
		public void Add(TObject obj)
		{
			_lock.EnterWriteLock();
			try
			{
				this.AddInternal(obj);
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		}

		/// <summary>
		/// Removes object from the spatial grid.
		/// </summary>
		/// <param name="obj"></param>
		public void Remove(TObject obj)
		{
			_lock.EnterWriteLock();
			try
			{
				this.RemoveInternal(obj);
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		}

		/// <summary>
		/// Removes all objects from the spatial grid.
		/// </summary>
		public void Clear()
		{
			_lock.EnterWriteLock();
			try
			{
				_cells.Clear();
				_objectCells.Clear();
				this.ObjectCount = 0;
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		}

		/// <summary>
		/// Updates the position of an object in the spatial grid.
		/// </summary>
		/// <param name="obj"></param>
		public void Update(TObject obj)
		{
			_lock.EnterWriteLock();
			try
			{
				this.RemoveInternal(obj);
				this.AddInternal(obj);
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		}

		/// <summary>
		/// Updates the position of all objects in the spatial grid.
		/// </summary>
		public void Update()
		{
			_lock.EnterWriteLock();
			try
			{
				foreach (var obj in _objectCells.Keys)
					_updateObjects.Add(obj);

				_cells.Clear();
				_objectCells.Clear();
				this.ObjectCount = 0;

				foreach (var obj in _updateObjects)
					this.AddInternal(obj);
			}
			finally
			{
				_updateObjects.Clear();
				_lock.ExitWriteLock();
			}
		}

		/// <summary>
		/// Returns a list of objects that intersect with the specified area.
		/// </summary>
		/// <param name="shape"></param>
		/// <returns></returns>
		public List<TObject> GetObjectsInArea(IShapeF shape)
		{
			var bounds = shape.GetBounds();
			return this.GetObjectsInArea(bounds.X, bounds.Y, bounds.Width, bounds.Height);
		}

		/// <summary>
		/// Adds objects that intersect with the specified area to the
		/// provided list.
		/// </summary>
		/// <param name="shape"></param>
		/// <param name="result"></param>
		public void GetObjectsInArea(IShapeF shape, List<TObject> result)
		{
			var bounds = shape.GetBounds();
			this.GetObjectsInArea(bounds.X, bounds.Y, bounds.Width, bounds.Height, result);
		}

		/// <summary>
		/// Returns a list of objects that intersect with the specified area.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns></returns>
		public List<TObject> GetObjectsInArea(float x, float y, float width, float height)
		{
			var result = new List<TObject>();
			this.GetObjectsInArea(x, y, width, height, result);

			return result;
		}

		/// <summary>
		/// Adds objects that intersect with the specified area to the
		/// provided list.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="result"></param>
		public void GetObjectsInArea(float x, float y, float width, float height, List<TObject> result)
		{
			var searchBounds = new BoundingBoxF(x, y, width, height);

			var minCoords = this.GetCoordinates(searchBounds.Left, searchBounds.Top);
			var maxCoords = this.GetCoordinates(searchBounds.Right, searchBounds.Bottom);

			if (!_hashSetPool.TryTake(out var seen))
				seen = new HashSet<TObject>();

			_lock.EnterReadLock();
			try
			{
				for (var cellX = minCoords.X; cellX <= maxCoords.X; cellX++)
				{
					for (var cellY = minCoords.Y; cellY <= maxCoords.Y; cellY++)
					{
						var coords = new Coordinates(cellX, cellY);

						if (!_cells.TryGetValue(coords, out var cell))
							continue;

						foreach (var obj in cell)
						{
							if (seen.Add(obj) && obj.Bounds.Intersects(searchBounds))
								result.Add(obj);
						}
					}
				}
			}
			finally
			{
				_lock.ExitReadLock();

				if (seen.Count <= this.PoolObjectThreshold)
				{
					seen.Clear();
					_hashSetPool.Add(seen);
				}
			}
		}

		/// <summary>
		/// Adds an object to the spatial grid.
		/// </summary>
		/// <param name="obj"></param>
		private void AddInternal(TObject obj)
		{
			if (_objectCells.ContainsKey(obj))
				return;

			var bounds = obj.Bounds;

			var minCoords = this.GetCoordinates(bounds.Left, bounds.Top);
			var maxCoords = this.GetCoordinates(bounds.Right, bounds.Bottom);

			for (var cellX = minCoords.X; cellX <= maxCoords.X; cellX++)
			{
				for (var cellY = minCoords.Y; cellY <= maxCoords.Y; cellY++)
				{
					var coords = new Coordinates(cellX, cellY);

					if (!_cells.TryGetValue(coords, out var cell))
						_cells[coords] = cell = new HashSet<TObject>();

					cell.Add(obj);
				}
			}

			_objectCells[obj] = new CellBounds(minCoords.X, minCoords.Y, maxCoords.X, maxCoords.Y);
			this.ObjectCount = _objectCells.Count;
		}

		/// <summary>
		/// Removes an object from the spatial grid.
		/// </summary>
		/// <param name="obj"></param>
		private void RemoveInternal(TObject obj)
		{
			if (!_objectCells.TryGetValue(obj, out var bounds))
				return;

			for (var cellX = bounds.MinX; cellX <= bounds.MaxX; cellX++)
			{
				for (var cellY = bounds.MinY; cellY <= bounds.MaxY; cellY++)
				{
					var coords = new Coordinates(cellX, cellY);

					if (_cells.TryGetValue(coords, out var cell))
					{
						cell.Remove(obj);
						if (cell.Count == 0)
							_cells.Remove(coords);
					}
				}
			}

			_objectCells.Remove(obj);
			this.ObjectCount = _objectCells.Count;
		}

		/// <summary>
		/// Returns the coordinates of the cell that contains the
		/// specified point.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		private Coordinates GetCoordinates(float x, float y)
		{
			var cellX = (int)Math.Floor(x / _cellSize);
			var cellY = (int)Math.Floor(y / _cellSize);

			return new Coordinates(cellX, cellY);
		}

		/// <summary>
		/// Describes the bounds of cells that an object occupies in the
		/// spatial grid.
		/// </summary>
		private readonly struct CellBounds
		{
			/// <summary>
			/// Minimum cell coordinates (inclusive).
			/// </summary>
			public readonly int MinX;

			/// <summary>
			/// Minimum cell coordinates (inclusive).
			/// </summary>
			public readonly int MinY;

			/// <summary>
			/// Maximum cell coordinates (inclusive).
			/// </summary>
			public readonly int MaxX;

			/// <summary>
			/// Maximum cell coordinates (inclusive).
			/// </summary>
			public readonly int MaxY;

			/// <summary>
			/// Creates new instance.
			/// </summary>
			/// <param name="minX"></param>
			/// <param name="minY"></param>
			/// <param name="maxX"></param>
			/// <param name="maxY"></param>
			public CellBounds(int minX, int minY, int maxX, int maxY)
			{
				this.MinX = minX;
				this.MinY = minY;
				this.MaxX = maxX;
				this.MaxY = maxY;
			}
		}

		/// <summary>
		/// Describes the coordinates of a cell in the spatial grid.
		/// </summary>
		private readonly struct Coordinates : IEquatable<Coordinates>
		{
			/// <summary>
			/// X coordinate of the cell.
			/// </summary>
			public readonly int X;

			/// <summary>
			/// Y coordinate of the cell.
			/// </summary>
			public readonly int Y;

			/// <summary>
			/// Creates new instance.
			/// </summary>
			/// <param name="x"></param>
			/// <param name="y"></param>
			public Coordinates(int x, int y)
			{
				this.X = x;
				this.Y = y;
			}

			/// <summary>
			/// Returns true if the other instance has the same
			/// coordinates as this one.
			/// </summary>
			/// <param name="other"></param>
			/// <returns></returns>
			public bool Equals(Coordinates other)
				=> this.X == other.X && this.Y == other.Y;

			/// <summary>
			/// Returns true if the other object is a Coordinates instance
			/// with the same coordinates as this one.
			/// </summary>
			/// <param name="obj"></param>
			/// <returns></returns>
			public override bool Equals(object obj)
				=> obj is Coordinates other && this.Equals(other);

			/// <summary>
			/// Returns a hash code based on the coordinates of the cell.
			/// </summary>
			/// <returns></returns>
			public override int GetHashCode()
			{
				unchecked
				{
					var hash = 17;
					hash = hash * 31 + this.X.GetHashCode();
					hash = hash * 31 + this.Y.GetHashCode();
					return hash;
				}
			}
		}
	}
}
