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

		[Fact]
		public void UpdatePosition()
		{
			var shape = new Cone(new Vector2(350, 350), 0, 200, 60);
			shape.UpdatePosition(new Vector2(250, 250));

			var expected = new Vector2[]
			{
				new Vector2(250, 250),
				new Vector2(423, 350),
				new Vector2(450, 250),
				new Vector2(423, 150),
			};

			Assert.Equal(expected, shape.GetEdgePoints());

			Assert.True(shape.IsInside(new Vector2(300, 250)));
			Assert.False(shape.IsInside(new Vector2(400, 350)));
			Assert.False(shape.IsInside(new Vector2(300, -250)));
		}
	}
}
