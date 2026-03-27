using Xunit;
using Yggdrasil.Geometry;
using Yggdrasil.Geometry.Shapes;
using Yggdrasil.Structures;

namespace Yggdrasil.Test.Structures
{
	public class SpatialGridTests
	{
		[Fact]
		public void GetObjectsInArea()
		{
			var grid = new SpatialGrid<Actor>(100);

			var actor1 = new Actor { Position = new Vector2F(50, 50), Size = 10 };
			var actor2 = new Actor { Position = new Vector2F(150, 150), Size = 10 };

			grid.Add(actor1);
			grid.Add(actor2);

			var objs = grid.GetObjectsInArea(0, 0, 100, 100);

			Assert.Contains(actor1, objs);
			Assert.DoesNotContain(actor2, objs);

			objs = grid.GetObjectsInArea(100, 100, 100, 100);

			Assert.DoesNotContain(actor1, objs);
			Assert.Contains(actor2, objs);

			objs = grid.GetObjectsInArea(0, 0, 200, 200);

			Assert.Contains(actor1, objs);
			Assert.Contains(actor2, objs);

			objs = grid.GetObjectsInArea(61, 61, 80, 80);

			Assert.DoesNotContain(actor1, objs);
			Assert.DoesNotContain(actor2, objs);
		}

		[Fact]
		public void GetObjectsInAreaNegative()
		{
			var grid = new SpatialGrid<Actor>(100);

			var actor1 = new Actor { Position = new Vector2F(-50, -50), Size = 10 };
			var actor2 = new Actor { Position = new Vector2F(150, 150), Size = 10 };

			grid.Add(actor1);
			grid.Add(actor2);

			var objs = grid.GetObjectsInArea(0, 0, 100, 100);

			Assert.DoesNotContain(actor1, objs);
			Assert.DoesNotContain(actor2, objs);

			objs = grid.GetObjectsInArea(-100, -100, 100, 100);

			Assert.Contains(actor1, objs);
			Assert.DoesNotContain(actor2, objs);
		}

		[Fact]
		public void GetObjectsInShape()
		{
			var grid = new SpatialGrid<Actor>(100);

			var actor1 = new Actor { Position = new Vector2F(50, 50), Size = 10 };
			var actor2 = new Actor { Position = new Vector2F(150, 150), Size = 10 };

			grid.Add(actor1);
			grid.Add(actor2);

			var objs = grid.GetObjectsInArea(new RectangleF(0, 0, 100, 100));

			Assert.Contains(actor1, objs);
			Assert.DoesNotContain(actor2, objs);

			objs = grid.GetObjectsInArea(new RectangleF(100, 100, 100, 100));

			Assert.DoesNotContain(actor1, objs);
			Assert.Contains(actor2, objs);

			objs = grid.GetObjectsInArea(new RectangleF(0, 0, 200, 200));

			Assert.Contains(actor1, objs);
			Assert.Contains(actor2, objs);

			objs = grid.GetObjectsInArea(new RectangleF(61, 61, 80, 80));

			Assert.DoesNotContain(actor1, objs);
			Assert.DoesNotContain(actor2, objs);

			objs = grid.GetObjectsInArea(PolygonF.RectangleBetween(new Vector2F(50, 50), new Vector2F(150, 150), 10));

			Assert.Contains(actor1, objs);
			Assert.Contains(actor2, objs);

			objs = grid.GetObjectsInArea(PolygonF.RectangleBetween(new Vector2F(70, 70), new Vector2F(130, 130), 10));

			Assert.DoesNotContain(actor1, objs);
			Assert.DoesNotContain(actor2, objs);
		}

		[Fact]
		public void Update()
		{
			var grid = new SpatialGrid<Actor>(100);

			var actor1 = new Actor { Position = new Vector2F(50, 50), Size = 10 };
			var actor2 = new Actor { Position = new Vector2F(150, 150), Size = 10 };

			grid.Add(actor1);
			grid.Add(actor2);

			var objs = grid.GetObjectsInArea(0, 0, 100, 100);

			Assert.Contains(actor1, objs);
			Assert.DoesNotContain(actor2, objs);

			actor1.Position = new Vector2F(140, 140);

			grid.Update(actor1);

			objs = grid.GetObjectsInArea(0, 0, 100, 100);

			Assert.DoesNotContain(actor1, objs);
			Assert.DoesNotContain(actor2, objs);

			objs = grid.GetObjectsInArea(100, 100, 100, 100);

			Assert.Contains(actor1, objs);
			Assert.Contains(actor2, objs);

			actor1.Position = new Vector2F(50, 50);
			actor2.Position = new Vector2F(40, 40);

			grid.Update();

			objs = grid.GetObjectsInArea(0, 0, 100, 100);

			Assert.Contains(actor1, objs);
			Assert.Contains(actor2, objs);
		}

		[Fact]
		public void MultiCell()
		{
			var grid = new SpatialGrid<Actor>(100);

			var actor1 = new Actor { Position = new Vector2F(50, 50), Size = 10 };
			var actor2 = new Actor { Position = new Vector2F(90, 90), Size = 20 };

			grid.Add(actor1);
			grid.Add(actor2);

			var objs = grid.GetObjectsInArea(0, 0, 89, 89);

			Assert.Contains(actor1, objs);
			Assert.DoesNotContain(actor2, objs);

			objs = grid.GetObjectsInArea(0, 0, 91, 91);

			Assert.Contains(actor1, objs);
			Assert.Contains(actor2, objs);

			objs = grid.GetObjectsInArea(105, 105, 100, 100);

			Assert.DoesNotContain(actor1, objs);
			Assert.Contains(actor2, objs);
		}

		private class Actor : ISpatialObject
		{
			public Vector2F Position { get; set; }
			public float Size { get; set; }

			public BoundingBoxF Bounds => new(this.Position.X, this.Position.Y, this.Size, this.Size);
		}
	}
}
