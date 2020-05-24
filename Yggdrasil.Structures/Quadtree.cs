// http://www.java2s.com/Open-Source/CSharp_Free_Code/Example/Download_C_QuadTree.htm
// Finally one that's working, one more problem and I'm gonna write my own... [exec]

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;

namespace Yggdrasil.Structures
{
	/// <summary>
	/// Describes an object that be put into QuadTree.
	/// </summary>
	public interface IQuadObject
	{
		/// <summary>
		/// Returns the bounding box of the object.
		/// </summary>
		RectangleF Bounds { get; }

		//event EventHandler BoundsChanged;
	}

	/// <summary>
	/// Specifies a direction.
	/// </summary>
	public enum Direction : int
	{
		/// <summary>
		/// North-west.
		/// </summary>
		NW = 0,

		/// <summary>
		/// North-east.
		/// </summary>
		NE = 1,

		/// <summary>
		/// South-west.
		/// </summary>
		SW = 2,

		/// <summary>
		/// South-east.
		/// </summary>
		SE = 3
	}

	/// <summary>
	/// A quadtree.
	/// </summary>
	/// <typeparam name="TQuadObject"></typeparam>
	public class Quadtree<TQuadObject> where TQuadObject : class, IQuadObject
	{
		private readonly object _syncLock = new object();

		private readonly bool _sort;
		private readonly Size _minLeafSize;
		private readonly int _maxObjectsPerLeaf;
		private QuadNode _root = null;
		private Dictionary<TQuadObject, QuadNode> _objectToNodeLookup = new Dictionary<TQuadObject, QuadNode>();
		private Dictionary<TQuadObject, int> _objectSortOrder = new Dictionary<TQuadObject, int>();
		private int _objectSortId = 0;

		/// <summary>
		/// Returns the tree's root node.
		/// </summary>
		public QuadNode Root => _root;

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="minLeafSize"></param>
		/// <param name="maxObjectsPerLeaf"></param>
		public Quadtree(Size minLeafSize, int maxObjectsPerLeaf)
		{
			_minLeafSize = minLeafSize;
			_maxObjectsPerLeaf = maxObjectsPerLeaf;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="minLeafSize">The smallest size a leaf will split into</param>
		/// <param name="maxObjectsPerLeaf">Maximum number of objects per leaf before it forces a split into sub quadrants</param>
		/// <param name="sort">Whether or not queries will return objects in the order in which they were added</param>
		public Quadtree(Size minLeafSize, int maxObjectsPerLeaf, bool sort)
			: this(minLeafSize, maxObjectsPerLeaf)
		{
			_sort = sort;
		}

		/// <summary>
		/// Returns the sort order of the given object.
		/// </summary>
		/// <param name="quadObject"></param>
		/// <returns></returns>
		public int GetSortOrder(TQuadObject quadObject)
		{
			lock (_objectSortOrder)
			{
				if (!_objectSortOrder.ContainsKey(quadObject))
					return -1;
				else
				{
					return _objectSortOrder[quadObject];
				}
			}
		}

		/// <summary>
		/// Adds object to the tree.
		/// </summary>
		/// <param name="quadObject"></param>
		public void Insert(TQuadObject quadObject)
		{
			lock (_syncLock)
			{
				if (_sort & !_objectSortOrder.ContainsKey(quadObject))
				{
					_objectSortOrder.Add(quadObject, _objectSortId++);
				}

				var bounds = quadObject.Bounds;
				if (_root == null)
				{
					var rootSize = new SizeF((float)Math.Ceiling(bounds.Width / _minLeafSize.Width),
											(float)Math.Ceiling(bounds.Height / _minLeafSize.Height));
					var multiplier = Math.Max(rootSize.Width, rootSize.Height);
					rootSize = new SizeF((_minLeafSize.Width * multiplier), (_minLeafSize.Height * multiplier));
					var center = new PointF(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);
					var rootOrigin = new PointF(center.X - rootSize.Width / 2, center.Y - rootSize.Height / 2);
					_root = new QuadNode(new RectangleF(rootOrigin, rootSize));
				}

				while (!_root.Bounds.Contains(bounds))
				{
					this.ExpandRoot(bounds);
				}

				this.InsertNodeObject(_root, quadObject);
			}
		}

		/// <summary>
		/// Returns a list of objects that intersect with the given bounds.
		/// </summary>
		/// <param name="bounds"></param>
		/// <returns></returns>
		public List<TQuadObject> Query(RectangleF bounds)
		{
			lock (_syncLock)
			{
				var results = new List<TQuadObject>();

				if (_root != null)
					this.Query(bounds, _root, results);

				if (_sort)
					results.Sort((a, b) => { return _objectSortOrder[a].CompareTo(_objectSortOrder[b]); });

				return results;
			}
		}

		/// <summary>
		/// Recursive method that writes the objects that intersect with
		/// the given bounds in the node to the given list.
		/// </summary>
		/// <param name="bounds"></param>
		/// <param name="node"></param>
		/// <param name="results"></param>
		private void Query(RectangleF bounds, QuadNode node, List<TQuadObject> results)
		{
			lock (_syncLock)
			{
				if (node == null) return;

				if (bounds.IntersectsWith(node.Bounds))
				{
					foreach (var quadObject in node.Objects)
					{
						if (bounds.IntersectsWith(quadObject.Bounds))
							results.Add(quadObject);
					}

					foreach (var childNode in node.Nodes)
					{
						this.Query(bounds, childNode, results);
					}
				}
			}
		}

		/// <summary>
		/// Expands root node.
		/// </summary>
		/// <param name="newChildBounds"></param>
		private void ExpandRoot(RectangleF newChildBounds)
		{
			lock (_syncLock)
			{
				var isNorth = _root.Bounds.Y < newChildBounds.Y;
				var isWest = _root.Bounds.X < newChildBounds.X;

				Direction rootDirection;
				if (isNorth)
				{
					rootDirection = isWest ? Direction.NW : Direction.NE;
				}
				else
				{
					rootDirection = isWest ? Direction.SW : Direction.SE;
				}

				double newX = (rootDirection == Direction.NW || rootDirection == Direction.SW)
								  ? _root.Bounds.X
								  : _root.Bounds.X - _root.Bounds.Width;
				double newY = (rootDirection == Direction.NW || rootDirection == Direction.NE)
								  ? _root.Bounds.Y
								  : _root.Bounds.Y - _root.Bounds.Height;

				var newRootBounds = new RectangleF((float)newX, (float)newY, _root.Bounds.Width * 2, _root.Bounds.Height * 2);
				var newRoot = new QuadNode(newRootBounds);
				this.SetupChildNodes(newRoot);
				newRoot[rootDirection] = _root;
				_root = newRoot;
			}
		}

		/// <summary>
		/// Inserts object in the node.
		/// </summary>
		/// <param name="node"></param>
		/// <param name="quadObject"></param>
		private void InsertNodeObject(QuadNode node, TQuadObject quadObject)
		{
			lock (_syncLock)
			{
				if (!node.Bounds.Contains(quadObject.Bounds))
					throw new Exception("This should not happen, child does not fit within node bounds");

				if (!node.HasChildNodes() && node.Objects.Count + 1 > _maxObjectsPerLeaf)
				{
					this.SetupChildNodes(node);

					var childObjects = new List<TQuadObject>(node.Objects);
					var childrenToRelocate = new List<TQuadObject>();

					foreach (var childObject in childObjects)
					{
						foreach (var childNode in node.Nodes)
						{
							if (childNode == null)
								continue;

							if (childNode.Bounds.Contains(childObject.Bounds))
							{
								childrenToRelocate.Add(childObject);
							}
						}
					}

					foreach (var childObject in childrenToRelocate)
					{
						this.RemoveQuadObjectFromNode(childObject);
						this.InsertNodeObject(node, childObject);
					}
				}

				foreach (var childNode in node.Nodes)
				{
					if (childNode != null)
					{
						if (childNode.Bounds.Contains(quadObject.Bounds))
						{
							this.InsertNodeObject(childNode, quadObject);
							return;
						}
					}
				}

				this.AddQuadObjectToNode(node, quadObject);
			}
		}

		/// <summary>
		/// Removes all objects from node.
		/// </summary>
		/// <param name="node"></param>
		private void ClearQuadObjectsFromNode(QuadNode node)
		{
			lock (_syncLock)
			{
				var quadObjects = new List<TQuadObject>(node.Objects);
				foreach (var quadObject in quadObjects)
				{
					this.RemoveQuadObjectFromNode(quadObject);
				}
			}
		}

		/// <summary>
		/// Removes object from its node.
		/// </summary>
		/// <param name="quadObject"></param>
		private void RemoveQuadObjectFromNode(TQuadObject quadObject)
		{
			lock (_syncLock)
			{
				var node = _objectToNodeLookup[quadObject];
				node._quadObjects.Remove(quadObject);
				_objectToNodeLookup.Remove(quadObject);
				//quadObject.BoundsChanged -= new EventHandler(quadObject_BoundsChanged);
			}
		}

		/// <summary>
		/// Adds object to node.
		/// </summary>
		/// <param name="node"></param>
		/// <param name="quadObject"></param>
		private void AddQuadObjectToNode(QuadNode node, TQuadObject quadObject)
		{
			lock (_syncLock)
			{
				node._quadObjects.Add(quadObject);
				_objectToNodeLookup.Add(quadObject, node);
				//quadObject.BoundsChanged += new EventHandler(quadObject_BoundsChanged);
			}
		}

		/// <summary>
		/// Called when an object's bounds changed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void QuadObject_BoundsChanged(object sender, EventArgs e)
		{
			lock (_syncLock)
			{
				if (sender is TQuadObject quadObject)
				{
					var node = _objectToNodeLookup[quadObject];
					if (!node.Bounds.Contains(quadObject.Bounds) || node.HasChildNodes())
					{
						this.RemoveQuadObjectFromNode(quadObject);
						this.Insert(quadObject);
						if (node.Parent != null)
						{
							this.CheckChildNodes(node.Parent);
						}
					}
				}
			}
		}

		/// <summary>
		/// Creates node's child nodes.
		/// </summary>
		/// <param name="node"></param>
		private void SetupChildNodes(QuadNode node)
		{
			lock (_syncLock)
			{
				if (_minLeafSize.Width <= node.Bounds.Width / 2 && _minLeafSize.Height <= node.Bounds.Height / 2)
				{
					node[Direction.NW] = new QuadNode(node.Bounds.X, node.Bounds.Y, node.Bounds.Width / 2,
													  node.Bounds.Height / 2);
					node[Direction.NE] = new QuadNode(node.Bounds.X + node.Bounds.Width / 2, node.Bounds.Y,
													  node.Bounds.Width / 2,
													  node.Bounds.Height / 2);
					node[Direction.SW] = new QuadNode(node.Bounds.X, node.Bounds.Y + node.Bounds.Height / 2,
													  node.Bounds.Width / 2,
													  node.Bounds.Height / 2);
					node[Direction.SE] = new QuadNode(node.Bounds.X + node.Bounds.Width / 2,
													  node.Bounds.Y + node.Bounds.Height / 2,
													  node.Bounds.Width / 2, node.Bounds.Height / 2);

				}
			}
		}

		/// <summary>
		/// Removes object from tree.
		/// </summary>
		/// <param name="quadObject"></param>
		public void Remove(TQuadObject quadObject)
		{
			lock (_syncLock)
			{
				if (_sort && _objectSortOrder.ContainsKey(quadObject))
				{
					_objectSortOrder.Remove(quadObject);
				}

				if (!_objectToNodeLookup.ContainsKey(quadObject))
					throw new KeyNotFoundException("QuadObject not found in dictionary for removal");

				var containingNode = _objectToNodeLookup[quadObject];
				this.RemoveQuadObjectFromNode(quadObject);

				if (containingNode.Parent != null)
					this.CheckChildNodes(containingNode.Parent);
			}
		}

		/// <summary>
		/// Reorganizes child nodes.
		/// </summary>
		/// <param name="node"></param>
		private void CheckChildNodes(QuadNode node)
		{
			lock (_syncLock)
			{
				if (this.GetQuadObjectCount(node) <= _maxObjectsPerLeaf)
				{
					// Move child objects into this node, and delete sub nodes
					var subChildObjects = this.GetChildObjects(node);
					foreach (var childObject in subChildObjects)
					{
						if (!node.Objects.Contains(childObject))
						{
							this.RemoveQuadObjectFromNode(childObject);
							this.AddQuadObjectToNode(node, childObject);
						}
					}
					if (node[Direction.NW] != null)
					{
						node[Direction.NW].Parent = null;
						node[Direction.NW] = null;
					}
					if (node[Direction.NE] != null)
					{
						node[Direction.NE].Parent = null;
						node[Direction.NE] = null;
					}
					if (node[Direction.SW] != null)
					{
						node[Direction.SW].Parent = null;
						node[Direction.SW] = null;
					}
					if (node[Direction.SE] != null)
					{
						node[Direction.SE].Parent = null;
						node[Direction.SE] = null;
					}

					if (node.Parent != null)
						this.CheckChildNodes(node.Parent);
					else
					{
						// Its the root node, see if we're down to one quadrant, with none in local storage - if so, ditch the other three
						var numQuadrantsWithObjects = 0;
						QuadNode nodeWithObjects = null;
						foreach (var childNode in node.Nodes)
						{
							if (childNode != null && this.GetQuadObjectCount(childNode) > 0)
							{
								numQuadrantsWithObjects++;
								nodeWithObjects = childNode;

								if (numQuadrantsWithObjects > 1)
									break;
							}
						}
						if (numQuadrantsWithObjects == 1)
						{
							foreach (var childNode in node.Nodes)
							{
								if (childNode != nodeWithObjects)
									childNode.Parent = null;
							}
							_root = nodeWithObjects;
						}
					}
				}
			}
		}

		/// <summary>
		/// Returns the node's and the node's children's node's objects.
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		private List<TQuadObject> GetChildObjects(QuadNode node)
		{
			lock (_syncLock)
			{
				var results = new List<TQuadObject>();
				results.AddRange(node._quadObjects);
				foreach (var childNode in node.Nodes)
				{
					if (childNode != null)
						results.AddRange(this.GetChildObjects(childNode));
				}
				return results;
			}
		}

		/// <summary>
		/// Returns the number of objects in the tree.
		/// </summary>
		/// <returns></returns>
		public int GetQuadObjectCount()
		{
			lock (_syncLock)
			{
				if (_root == null)
					return 0;

				var count = this.GetQuadObjectCount(_root);
				return count;
			}
		}

		/// <summary>
		/// Returns the number of objects in the node.
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		private int GetQuadObjectCount(QuadNode node)
		{
			lock (_syncLock)
			{
				var count = node.Objects.Count;
				foreach (var childNode in node.Nodes)
				{
					if (childNode != null)
					{
						count += this.GetQuadObjectCount(childNode);
					}
				}
				return count;
			}
		}

		/// <summary>
		/// Returns the number of nodes in the tree.
		/// </summary>
		/// <returns></returns>
		public int GetQuadNodeCount()
		{
			lock (_syncLock)
			{
				if (_root == null)
					return 0;
				var count = this.GetQuadNodeCount(_root, 1);
				return count;
			}
		}

		/// <summary>
		/// Returns the number of child nodes in the given node.
		/// </summary>
		/// <param name="node"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		private int GetQuadNodeCount(QuadNode node, int count)
		{
			lock (_syncLock)
			{
				if (node == null) return count;

				foreach (var childNode in node.Nodes)
				{
					if (childNode != null)
						count++;
				}
				return count;
			}
		}

		/// <summary>
		/// Returns a list with all nodes in the tree.
		/// </summary>
		/// <returns></returns>
		public List<QuadNode> GetAllNodes()
		{
			lock (_syncLock)
			{
				var results = new List<QuadNode>();
				if (_root != null)
				{
					results.Add(_root);
					this.GetChildNodes(_root, results);
				}
				return results;
			}
		}

		/// <summary>
		/// Recursive method that Writes all nodes and their children in
		/// the given node to the list.
		/// </summary>
		/// <param name="node"></param>
		/// <param name="results"></param>
		private void GetChildNodes(QuadNode node, ICollection<QuadNode> results)
		{
			lock (_syncLock)
			{
				foreach (var childNode in node.Nodes)
				{
					if (childNode != null)
					{
						results.Add(childNode);
						this.GetChildNodes(childNode, results);
					}
				}
			}
		}

		/// <summary>
		/// Represents a note in a quad tree.
		/// </summary>
		public class QuadNode
		{
			//private static int _id = 0;
			//public readonly int ID = _id++;

			private readonly QuadNode[] _nodes = new QuadNode[4];
			internal List<TQuadObject> _quadObjects = new List<TQuadObject>();

			/// <summary>
			/// Returns the node's parent.
			/// </summary>
			public QuadNode Parent { get; internal set; }

			/// <summary>
			/// Returns a list of the node's nodes.
			/// </summary>
			public ReadOnlyCollection<QuadNode> Nodes;

			/// <summary>
			/// Returns a list of the node's objects.
			/// </summary>
			public ReadOnlyCollection<TQuadObject> Objects;

			/// <summary>
			/// Returns the node's bounds.
			/// </summary>
			public RectangleF Bounds { get; internal set; }

			/// <summary>
			/// Creates new instace.
			/// </summary>
			/// <param name="bounds"></param>
			public QuadNode(RectangleF bounds)
			{
				this.Bounds = bounds;
				this.Nodes = new ReadOnlyCollection<QuadNode>(_nodes);
				this.Objects = new ReadOnlyCollection<TQuadObject>(_quadObjects);
			}

			/// <summary>
			/// Creates new instance.
			/// </summary>
			/// <param name="x"></param>
			/// <param name="y"></param>
			/// <param name="width"></param>
			/// <param name="height"></param>
			public QuadNode(double x, double y, double width, double height)
				: this(new RectangleF((float)x, (float)y, (float)width, (float)height))
			{

			}

			/// <summary>
			/// Returns true if the node has child nodes.
			/// </summary>
			/// <returns></returns>
			public bool HasChildNodes()
			{
				return _nodes[0] != null;
			}

			/// <summary>
			/// Gets or sets the child node in the given direction.
			/// </summary>
			/// <param name="direction"></param>
			/// <returns></returns>
			public QuadNode this[Direction direction]
			{
				get
				{
					switch (direction)
					{
						case Direction.NW:
							return _nodes[0];
						case Direction.NE:
							return _nodes[1];
						case Direction.SW:
							return _nodes[2];
						case Direction.SE:
							return _nodes[3];
						default:
							return null;
					}
				}
				set
				{
					switch (direction)
					{
						case Direction.NW:
							_nodes[0] = value;
							break;
						case Direction.NE:
							_nodes[1] = value;
							break;
						case Direction.SW:
							_nodes[2] = value;
							break;
						case Direction.SE:
							_nodes[3] = value;
							break;
					}
					if (value != null)
						value.Parent = this;
				}
			}
		}
	}
}
