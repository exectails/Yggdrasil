using Xunit;
using Yggdrasil.Geometry;

namespace Yggdrasil.Test.Geometry
{
	public class BoundingBoxTests
	{
		[Fact]
		public void Contains()
		{
			var box = new BoundingBoxF(0, 0, 100, 100);

			Assert.True(box.Contains(50, 50));
			Assert.True(box.Contains(0, 0));
			Assert.True(box.Contains(100, 100));
			Assert.False(box.Contains(-1, -1));
			Assert.False(box.Contains(101, 101));

			Assert.True(box.Contains(new Vector2F(50, 50)));
			Assert.True(box.Contains(new Vector2F(0, 0)));
			Assert.True(box.Contains(new Vector2F(100, 100)));
			Assert.False(box.Contains(new Vector2F(-1, -1)));
			Assert.False(box.Contains(new Vector2F(101, 101)));
		}

		[Fact]
		public void Intersects()
		{
			var box1 = new BoundingBoxF(0, 0, 100, 100);
			var box2 = new BoundingBoxF(50, 50, 100, 100);
			var box3 = new BoundingBoxF(100, 100, 100, 100);
			var box4 = new BoundingBoxF(101, 101, 100, 100);

			Assert.True(box1.Intersects(box2));
			Assert.True(box1.Intersects(box3));
			Assert.False(box1.Intersects(box4));
		}

		[Fact]
		public void FromPoints()
		{
			var points = new Vector2F[]
			{
				new(10, 20),
				new(30, 40),
				new(50, 60),
			};

			var box = BoundingBoxF.FromPoints(points);

			Assert.Equal(10, box.X);
			Assert.Equal(20, box.Y);
			Assert.Equal(40, box.Width);
			Assert.Equal(40, box.Height);
		}
	}
}
