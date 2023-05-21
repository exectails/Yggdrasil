using Yggdrasil.Geometry;
using Yggdrasil.Geometry.Shapes;
using Xunit;
using System;

namespace Yggdrasil.Tests.Geometry.Shapes
{
	public class CircleTests
	{
		[Fact]
		public void IsInside()
		{
			var shape = new Circle(new Vector2(500, 500), 200);
			Assert.True(shape.IsInside(new Vector2(500, 500)));
			Assert.True(shape.IsInside(new Vector2(500, 600)));
			Assert.True(shape.IsInside(new Vector2(500, 400)));
			Assert.True(shape.IsInside(new Vector2(600, 500)));
			Assert.True(shape.IsInside(new Vector2(400, 500)));
			Assert.True(shape.IsInside(new Vector2(450, 450)));
			Assert.True(shape.IsInside(new Vector2(550, 550)));
			Assert.False(shape.IsInside(new Vector2(350, 650)));
			Assert.False(shape.IsInside(new Vector2(650, 350)));
		}

		[Fact]
		public void GetRandomPosition()
		{
			var shape = new Circle(new Vector2(500, 500), 0);
			var rnd = new Random();

			var rndPos = shape.GetRandomPoint(rnd);
			Assert.Equal(500, rndPos.X);
			Assert.Equal(500, rndPos.Y);
			Assert.True(shape.IsInside(rndPos));
		}

		[Fact]
		public void GetEdgePoints()
		{
			var shape = new Circle(new Vector2(500, 500), 200);
			var expected = new Vector2[]
			{
				new Vector2(300, 500),
				new Vector2(500, 300),
				new Vector2(700, 500),
				new Vector2(500, 700),
			};

			Assert.Equal(expected, shape.GetEdgePoints(4));

			expected = new Vector2[]
			{
				new Vector2(358, 641),
				new Vector2(300, 499),
				new Vector2(358, 358),
				new Vector2(500, 300),
				new Vector2(641, 358),
				new Vector2(700, 500),
				new Vector2(641, 641),
				new Vector2(499, 700),
			};

			//for (var i = 0; i < 8; ++i)
			//	Assert.Equal(expected[i], shape.GetEdgePoints(8)[i]);

			Assert.Equal(expected, shape.GetEdgePoints(8));
		}
	}
}
