using Yggdrasil.Geometry;
using Yggdrasil.Geometry.Shapes;
using Xunit;
using System;

namespace Yggdrasil.Tests.Geometry.Shapes
{
	public class CircleFTests
	{
		[Fact]
		public void IsInside()
		{
			var shape = new CircleF(new Vector2F(500, 500), 200);
			Assert.True(shape.IsInside(new Vector2F(500, 500)));
			Assert.True(shape.IsInside(new Vector2F(500, 600)));
			Assert.True(shape.IsInside(new Vector2F(500, 400)));
			Assert.True(shape.IsInside(new Vector2F(600, 500)));
			Assert.True(shape.IsInside(new Vector2F(400, 500)));
			Assert.True(shape.IsInside(new Vector2F(450, 450)));
			Assert.True(shape.IsInside(new Vector2F(550, 550)));
			Assert.False(shape.IsInside(new Vector2F(350, 650)));
			Assert.False(shape.IsInside(new Vector2F(650, 350)));
		}

		[Fact]
		public void GetRandomPosition()
		{
			var shape = new CircleF(new Vector2F(500, 500), 0);
			var rnd = new Random();

			var rndPos = shape.GetRandomPoint(rnd);
			Assert.Equal(500, rndPos.X);
			Assert.Equal(500, rndPos.Y);
			Assert.True(shape.IsInside(rndPos));
		}

		[Fact]
		public void GetEdgePoints()
		{
			var shape = new CircleF(new Vector2F(500, 500), 200);
			var expected = new Vector2F[]
			{
				new Vector2F(300, 500),
				new Vector2F(500, 300),
				new Vector2F(700, 500),
				new Vector2F(500, 700),
			};

			Assert.Equal(expected, shape.GetEdgePoints(4));

			expected = new Vector2F[]
			{
				new Vector2F(358.58f, 641.42f),
				new Vector2F(300.00f, 500.00f),
				new Vector2F(358.58f, 358.58f),
				new Vector2F(500.00f, 300.00f),
				new Vector2F(641.42f, 358.58f),
				new Vector2F(700.00f, 500.00f),
				new Vector2F(641.42f, 641.42f),
				new Vector2F(500.00f, 700.00f),
			};

			var edgePoints = shape.GetEdgePoints(8);
			for (var i = 0; i < 8; ++i)
			{
				Assert.Equal(expected[i].X, edgePoints[i].X, 2);
				Assert.Equal(expected[i].Y, edgePoints[i].Y, 2);
			}
		}
	}
}
