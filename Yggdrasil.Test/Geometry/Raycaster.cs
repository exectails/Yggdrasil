using System.Collections.Generic;
using Xunit;
using Yggdrasil.Geometry;
using Yggdrasil.Geometry.Shapes;

namespace Yggdrasil.Test.Geometry
{
	public class RaycasterTests
	{
		[Fact]
		public void RaycastCircle_Hit()
		{
			var circle = new CircleF(new Vector2F(10, 0), 5);
			var result = Raycaster.Raycast(new Vector2F(0, 0), new Vector2F(1, 0), circle);

			Assert.True(result.Hit);
			Assert.Equal(5, result.Distance);
			Assert.Equal(new Vector2F(5, 0), result.Point);
			Assert.Equal(new Vector2F(-1, 0), result.Normal);
			Assert.Same(circle, result.Shape);
		}

		[Fact]
		public void RaycastCircle_Miss()
		{
			var circle = new CircleF(new Vector2F(10, 10), 1);
			var result = Raycaster.Raycast(new Vector2F(0, 0), new Vector2F(1, 0), circle);

			Assert.False(result.Hit);
			Assert.Equal(0, result.Distance);
		}

		[Fact]
		public void RaycastCircle_Inside()
		{
			var circle = new CircleF(new Vector2F(0, 0), 5);
			var result = Raycaster.Raycast(new Vector2F(0, 0), new Vector2F(1, 0), circle);

			Assert.True(result.Hit);
			Assert.Equal(5, result.Distance);
			Assert.Equal(new Vector2F(5, 0), result.Point);
			Assert.Equal(new Vector2F(1, 0), result.Normal);
			Assert.Same(circle, result.Shape);
		}

		[Fact]
		public void RaycastPolygon_Hit()
		{
			var polygon = new PolygonF(
				new Vector2F(5, -5),
				new Vector2F(15, -5),
				new Vector2F(15, 5),
				new Vector2F(5, 5)
			);

			var result = Raycaster.Raycast(new Vector2F(0, 0), new Vector2F(1, 0), polygon);

			Assert.True(result.Hit);
			Assert.Equal(5, result.Distance);
			Assert.Equal(new Vector2F(5, 0), result.Point);
			Assert.Equal(new Vector2F(-1, 0), result.Normal);
			Assert.Same(polygon, result.Shape);
		}

		[Fact]
		public void RaycastPolygon_Miss()
		{
			var polygon = new PolygonF(
				new Vector2F(5, 5),
				new Vector2F(15, 5),
				new Vector2F(15, 15),
				new Vector2F(5, 15)
			);

			var result = Raycaster.Raycast(new Vector2F(0, 0), new Vector2F(1, 0), polygon);

			Assert.False(result.Hit);
			Assert.Equal(0, result.Distance);
		}

		[Fact]
		public void Raycast_ClosestHit()
		{
			var circle = new CircleF(new Vector2F(10, 0), 2);
			var polygon = new PolygonF(
				new Vector2F(15, -5),
				new Vector2F(25, -5),
				new Vector2F(25, 5),
				new Vector2F(15, 5)
			);

			var shapes = new List<IShapeF> { polygon, circle };

			var result = Raycaster.Raycast(new Vector2F(0, 0), new Vector2F(1, 0), shapes);

			Assert.True(result.Hit);
			Assert.Equal(8, result.Distance);
			Assert.Equal(new Vector2F(8, 0), result.Point);
			Assert.Same(circle, result.Shape);
		}
	}
}
