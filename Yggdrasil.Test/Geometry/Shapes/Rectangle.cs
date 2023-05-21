using Yggdrasil.Geometry;
using Yggdrasil.Geometry.Shapes;
using Xunit;

namespace Yggdrasil.Tests.Geometry.Shapes
{
	public class RectangleTests
	{
		[Fact]
		public void Create()
		{
			var shape = new Rectangle(new Vector2(400, 400), new Vector2(200, 200));
			Assert.Equal(new Vector2(400, 400), shape.Position);
			Assert.Equal(new Vector2(500, 500), shape.Center);
			Assert.Equal(new Vector2(200, 200), shape.Size);

			shape = Rectangle.Centered(new Vector2(500, 500), new Vector2(200, 200));
			Assert.Equal(new Vector2(400, 400), shape.Position);
			Assert.Equal(new Vector2(500, 500), shape.Center);
			Assert.Equal(new Vector2(200, 200), shape.Size);
		}

		[Fact]
		public void IsInside()
		{
			var shape = Rectangle.Centered(new Vector2(500, 500), new Vector2(200, 200));
			Assert.True(shape.IsInside(new Vector2(500, 500)));
			Assert.True(shape.IsInside(new Vector2(450, 450)));
			Assert.True(shape.IsInside(new Vector2(400, 400)));
			Assert.True(shape.IsInside(new Vector2(550, 550)));
			Assert.True(shape.IsInside(new Vector2(500, 500)));
			Assert.False(shape.IsInside(new Vector2(399, 399)));
			Assert.False(shape.IsInside(new Vector2(10, 10)));
			Assert.False(shape.IsInside(new Vector2(601, 601)));
			Assert.False(shape.IsInside(new Vector2(1000, 1000)));
		}

		[Fact]
		public void GetEdgePoints()
		{
			var shape = Rectangle.Centered(new Vector2(500, 500), new Vector2(200, 200));
			var expected = new Vector2[4]
			{
				new Vector2(400, 400),
				new Vector2(600, 400),
				new Vector2(600, 600),
				new Vector2(400, 600),
			};

			Assert.Equal(expected, shape.GetEdgePoints());
		}
	}
}
