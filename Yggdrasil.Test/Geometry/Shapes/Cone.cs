using Yggdrasil.Geometry;
using Yggdrasil.Geometry.Shapes;
using Xunit;

namespace Yggdrasil.Tests.Geometry.Shapes
{
	public class ConeTests
	{
		[Fact]
		public void IsInside()
		{
			var shape = new Cone(new Vector2(350, 350), 0, 200, 60);
			Assert.True(shape.IsInside(new Vector2(400, 350)));

			shape = new Cone(new Vector2(350, 350), 0, 200, 0);
			Assert.False(shape.IsInside(new Vector2(400, 350)));
			Assert.False(shape.IsInside(new Vector2(400, 340)));

			shape = new Cone(new Vector2(350, 350), 0, 200, 1);
			Assert.True(shape.IsInside(new Vector2(400, 350)));
			Assert.False(shape.IsInside(new Vector2(400, 340)));
		}

		[Fact]
		public void GetEdgePoints()
		{
			var shape = new Cone(new Vector2(350, 350), 0, 200, 60);
			var expected = new Vector2[]
			{
				new Vector2(350, 350),
				new Vector2(523, 450),
				new Vector2(550, 350),
				new Vector2(523, 250),
			};

			Assert.Equal(expected, shape.GetEdgePoints());
		}
	}
}
