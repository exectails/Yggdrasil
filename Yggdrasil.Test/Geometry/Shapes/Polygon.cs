using Yggdrasil.Geometry;
using Yggdrasil.Geometry.Shapes;
using Xunit;

namespace Yggdrasil.Tests.Geometry.Shapes
{
	public class PolygonTests
	{
		[Fact]
		public void IsInside()
		{
			// Triangle
			var shape = new Polygon(new Vector2(500, 500), new Vector2(400, 300), new Vector2(600, 300));
			Assert.True(shape.IsInside(new Vector2(500, 400)));
			Assert.True(shape.IsInside(new Vector2(410, 301)));
			Assert.True(shape.IsInside(new Vector2(590, 301)));
			Assert.True(shape.IsInside(new Vector2(410, 301)));
			Assert.False(shape.IsInside(new Vector2(590, 490)));
			Assert.False(shape.IsInside(new Vector2(410, 490)));
		}

		[Fact]
		public void GetEdgePoints()
		{
			// Triangle
			var shape = new Polygon(new Vector2(500, 500), new Vector2(400, 300), new Vector2(600, 300));
			var expected = new Vector2[]
			{
				new Vector2(500, 500),
				new Vector2(400, 300),
				new Vector2(600, 300),
			};

			Assert.Equal(expected, shape.GetEdgePoints());
		}

		[Fact]
		public void UpdatePosition()
		{
			// Triangle
			var shape = new Polygon(new Vector2(500, 500), new Vector2(400, 300), new Vector2(600, 300));
			shape.UpdatePosition(new Vector2(-100, -100));

			var expected = new Vector2[]
			{
				new Vector2(-100, 34),
				new Vector2(-200, -166),
				new Vector2(0, -166),
			};

			Assert.Equal(expected, shape.GetEdgePoints());

			Assert.False(shape.IsInside(new Vector2(-100, -200)));
			Assert.False(shape.IsInside(new Vector2(-210, -301)));
			Assert.False(shape.IsInside(new Vector2(-190, -301)));
			Assert.False(shape.IsInside(new Vector2(-210, -301)));
			Assert.False(shape.IsInside(new Vector2(-190, -290)));
			Assert.False(shape.IsInside(new Vector2(-210, -290)));
			Assert.True(shape.IsInside(new Vector2(-150, -100)));
			Assert.True(shape.IsInside(new Vector2(-50, -100)));
			Assert.True(shape.IsInside(new Vector2(-100, -150)));
		}
	}
}
