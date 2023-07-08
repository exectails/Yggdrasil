using Yggdrasil.Geometry;
using Yggdrasil.Geometry.Shapes;
using Xunit;

namespace Yggdrasil.Tests.Geometry.Shapes
{
	public class ConeFTests
	{
		[Fact]
		public void IsInside()
		{
			var shape = new ConeF(new Vector2F(350, 350), 0, 200, 60);
			Assert.True(shape.IsInside(new Vector2F(400, 350)));

			shape = new ConeF(new Vector2F(350, 350), 0, 200, 0);
			Assert.False(shape.IsInside(new Vector2F(400, 350)));
			Assert.False(shape.IsInside(new Vector2F(400, 340)));

			shape = new ConeF(new Vector2F(350, 350), 0, 200, 1);
			Assert.True(shape.IsInside(new Vector2F(400, 350)));
			Assert.False(shape.IsInside(new Vector2F(400, 340)));
		}

		[Fact]
		public void GetEdgePoints()
		{
			var shape = new ConeF(new Vector2F(350, 350), 0, 200, 60);
			var expected = new Vector2F[]
			{
				new Vector2F(350, 350),
				new Vector2F(523, 450),
				new Vector2F(550, 350),
				new Vector2F(523, 250),
			};

			Assert.Equal(expected, shape.GetEdgePoints());
		}
	}
}
