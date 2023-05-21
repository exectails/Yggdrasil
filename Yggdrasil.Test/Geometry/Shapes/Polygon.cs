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
	}
}
