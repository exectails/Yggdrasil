using Yggdrasil.Geometry;
using Yggdrasil.Geometry.Shapes;
using Xunit;

namespace Yggdrasil.Tests.Geometry.Shapes
{
	public class RectangleFTests
	{
		[Fact]
		public void Create()
		{
			var shape = new RectangleF(new Vector2F(400, 400), new Vector2F(200, 200));
			Assert.Equal(new Vector2F(400, 400), shape.Position);
			Assert.Equal(new Vector2F(500, 500), shape.Center);
			Assert.Equal(new Vector2F(200, 200), shape.Size);

			shape = RectangleF.Centered(new Vector2F(500, 500), new Vector2F(200, 200));
			Assert.Equal(new Vector2F(400, 400), shape.Position);
			Assert.Equal(new Vector2F(500, 500), shape.Center);
			Assert.Equal(new Vector2F(200, 200), shape.Size);
		}

		[Fact]
		public void IsInside()
		{
			var shape = RectangleF.Centered(new Vector2F(500, 500), new Vector2F(200, 200));
			Assert.True(shape.IsInside(new Vector2F(500, 500)));
			Assert.True(shape.IsInside(new Vector2F(450, 450)));
			Assert.True(shape.IsInside(new Vector2F(400, 400)));
			Assert.True(shape.IsInside(new Vector2F(550, 550)));
			Assert.True(shape.IsInside(new Vector2F(500, 500)));
			Assert.False(shape.IsInside(new Vector2F(399, 399)));
			Assert.False(shape.IsInside(new Vector2F(10, 10)));
			Assert.False(shape.IsInside(new Vector2F(601, 601)));
			Assert.False(shape.IsInside(new Vector2F(1000, 1000)));
		}

		[Fact]
		public void GetEdgePoints()
		{
			var shape = RectangleF.Centered(new Vector2F(500, 500), new Vector2F(200, 200));
			var expected = new Vector2F[4]
			{
				new Vector2F(400, 400),
				new Vector2F(600, 400),
				new Vector2F(600, 600),
				new Vector2F(400, 600),
			};

			Assert.Equal(expected, shape.GetEdgePoints());
		}
	}
}
