using System;
using System.Collections.Generic;
using System.Linq;
using Yggdrasil.Extensions;

namespace Yggdrasil.Geometry.Shapes
{
	/// <summary>
	/// A shape that combines multiple shapes.
	/// </summary>
	public class CompositeShapeF : IShapeF
	{
		private Vector2F[] _edgePoints;
		private OutlineF[] _outlines;

		/// <summary>
		/// Returns all shapes this composite shape contains.
		/// </summary>
		public IShapeF[] Shapes { get; }

		/// <summary>
		/// Returns the average of the combined shapes.
		/// </summary>
		public Vector2F Center { get; }

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="shapes"></param>
		public CompositeShapeF(params IShape[] shapes)
			: this((IEnumerable<IShapeF>)shapes)
		{
		}

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="shapes"></param>
		public CompositeShapeF(IEnumerable<IShapeF> shapes)
		{
			this.Shapes = shapes.ToArray();

			if (this.Shapes.Length == 1)
			{
				this.Center = this.Shapes[0].Center;
			}
			else
			{
				var count = this.Shapes.Length;
				var xSum = shapes.Sum(a => a.Center.X);
				var ySum = shapes.Sum(a => a.Center.Y);
				var x = xSum / count;
				var y = ySum / count;

				this.Center = new Vector2F(x, y);
			}
		}

		/// <summary>
		/// Returns true if the given position inside any of this composite
		/// shape's shapes.
		/// </summary>
		/// <param name="pos"></param>
		/// <returns></returns>
		public bool IsInside(Vector2F pos)
		{
			for (var i = 0; i < this.Shapes.Length; ++i)
			{
				if (this.Shapes[i].IsInside(pos))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Returns a list of all edge points of all shapes.
		/// </summary>
		/// <returns></returns>
		public Vector2F[] GetEdgePoints()
		{
			if (_edgePoints == null)
				_edgePoints = this.Shapes.SelectMany(a => a.GetEdgePoints()).ToArray();

			return _edgePoints;
		}

		/// <summary>
		/// Returns the outlines for all sub-shapes.
		/// </summary>
		/// <returns></returns>
		public OutlineF[] GetOutlines()
		{
			if (_outlines != null)
				return _outlines;

			return _outlines = this.Shapes.SelectMany(a => a.GetOutlines()).ToArray();
		}

		/// <summary>
		/// Returns a random point within one of this shape's shapes.
		/// </summary>
		/// <param name="rnd"></param>
		/// <returns></returns>
		public Vector2F GetRandomPoint(Random rnd)
		{
			var rndShape = this.Shapes.Random();
			return rndShape.GetRandomPoint(rnd);
		}
	}
}
