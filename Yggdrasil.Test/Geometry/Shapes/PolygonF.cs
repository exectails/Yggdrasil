﻿using Yggdrasil.Geometry;
using Yggdrasil.Geometry.Shapes;
using Xunit;

namespace Yggdrasil.Tests.Geometry.Shapes
{
	public class PolygonFTests
	{
		[Fact]
		public void IsInside()
		{
			// Triangle
			var shape = new PolygonF(new Vector2F(500, 500), new Vector2F(400, 300), new Vector2F(600, 300));
			Assert.True(shape.IsInside(new Vector2F(500, 400)));
			Assert.True(shape.IsInside(new Vector2F(410, 301)));
			Assert.True(shape.IsInside(new Vector2F(590, 301)));
			Assert.True(shape.IsInside(new Vector2F(410, 301)));
			Assert.False(shape.IsInside(new Vector2F(590, 490)));
			Assert.False(shape.IsInside(new Vector2F(410, 490)));
		}

		[Fact]
		public void GetEdgePoints()
		{
			// Triangle
			var shape = new PolygonF(new Vector2F(500, 500), new Vector2F(400, 300), new Vector2F(600, 300));
			var expected = new Vector2F[]
			{
				new Vector2F(500, 500),
				new Vector2F(400, 300),
				new Vector2F(600, 300),
			};

			Assert.Equal(expected, shape.GetEdgePoints());
		}

		[Fact]
		public void UpdatePosition()
		{
			// Triangle
			var shape = new PolygonF(new Vector2F(500, 500), new Vector2F(400, 300), new Vector2F(600, 300));
			shape.UpdatePosition(new Vector2F(-100, -100));

			var expected = new Vector2F[]
			{
				new Vector2F(-100, -100),
				new Vector2F(-200, -300),
				new Vector2F(0, -300),
			};

			Assert.Equal(expected, shape.GetEdgePoints());

			Assert.True(shape.IsInside(new Vector2F(-100, -200)));
			Assert.True(shape.IsInside(new Vector2F(-210, -301)));
			Assert.True(shape.IsInside(new Vector2F(-190, -301)));
			Assert.True(shape.IsInside(new Vector2F(-210, -301)));
			Assert.False(shape.IsInside(new Vector2F(-190, -290)));
			Assert.False(shape.IsInside(new Vector2F(-210, -290)));
		}
	}
}
