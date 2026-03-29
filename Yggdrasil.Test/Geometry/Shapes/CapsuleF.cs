using System;
using Xunit;
using Yggdrasil.Geometry;
using Yggdrasil.Geometry.Shapes;

namespace Yggdrasil.Test.Geometry.Shapes
{
	public class CapsuleFTests
	{
		[Fact]
		public void IsInside()
		{
			var shape = new CapsuleF(new Vector2F(100, 100), new Vector2F(300, 100), 50, 200);

			Assert.True(shape.IsInside(new Vector2F(100, 100)));
			Assert.True(shape.IsInside(new Vector2F(300, 100)));
			Assert.True(shape.IsInside(new Vector2F(200, 100)));

			Assert.True(shape.IsInside(new Vector2F(200, 150)));
			Assert.True(shape.IsInside(new Vector2F(200, 50)));

			Assert.True(shape.IsInside(new Vector2F(50, 100)));
			Assert.True(shape.IsInside(new Vector2F(350, 100)));

			Assert.False(shape.IsInside(new Vector2F(200, 151)));
			Assert.False(shape.IsInside(new Vector2F(200, 49)));
			Assert.False(shape.IsInside(new Vector2F(49, 100)));
			Assert.False(shape.IsInside(new Vector2F(351, 100)));
		}

		[Fact]
		public void GetRandomPoint()
		{
			var shape = new CapsuleF(new Vector2F(100, 100), new Vector2F(300, 100), 50, 200);
			var rnd = new Random();

			for (var i = 0; i < 100; ++i)
			{
				var rndPos = shape.GetRandomPoint(rnd);
				Assert.True(shape.IsInside(rndPos));
			}
		}

		[Fact]
		public void UpdatePosition()
		{
			var shape = new CapsuleF(new Vector2F(100, 100), new Vector2F(300, 100), 50, 200);

			shape.UpdatePosition(new Vector2F(500, 500));

			Assert.Equal(400, shape.Point1.X);
			Assert.Equal(500, shape.Point1.Y);

			Assert.Equal(600, shape.Point2.X);
			Assert.Equal(500, shape.Point2.Y);

			Assert.Equal(500, shape.Center.X);
			Assert.Equal(500, shape.Center.Y);

			Assert.True(shape.IsInside(new Vector2F(500, 500)));
			Assert.True(shape.IsInside(new Vector2F(400, 500)));
			Assert.True(shape.IsInside(new Vector2F(600, 500)));
			Assert.True(shape.IsInside(new Vector2F(500, 550)));

			Assert.False(shape.IsInside(new Vector2F(500, 551)));
		}
	}
}
