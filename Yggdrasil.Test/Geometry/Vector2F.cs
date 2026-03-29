using Yggdrasil.Geometry;
using Xunit;

namespace Yggdrasil.Test.Geometry
{
	public class Vector2FTests
	{
		[Fact]
		public void Operators()
		{
			var vector1 = new Vector2F(10, 10);
			var vector2 = new Vector2F(10, 10);

			var vector3 = vector1 + vector2;
			Assert.Equal(new Vector2F(20, 20), vector3);

			vector3 -= vector2;
			Assert.Equal(new Vector2F(10, 10), vector3);

			vector3 -= vector2;
			Assert.Equal(new Vector2F(0, 0), vector3);

			vector3 *= 100;
			Assert.Equal(new Vector2F(0, 0), vector3);

			vector3 += new Vector2F(50, 50);
			Assert.Equal(new Vector2F(50, 50), vector3);

			vector3 *= 2;
			Assert.Equal(new Vector2F(100, 100), vector3);

			vector3 /= 4;
			Assert.Equal(new Vector2F(25, 25), vector3);
		}

		[Fact]
		public void GetDistance()
		{
			var vector1 = new Vector2F(0, 0);
			var vector2 = new Vector2F(3, 4);

			Assert.Equal(5, vector1.GetDistance(vector2));
			Assert.Equal(5, vector2.GetDistance(vector1));

			Assert.Equal(0, vector1.GetDistance(vector1));

			var vector3 = new Vector2F(-3, -4);
			Assert.Equal(5, vector1.GetDistance(vector3));
			Assert.Equal(10, vector2.GetDistance(vector3));
		}

		[Fact]
		public void InRange()
		{
			var vector1 = new Vector2F(0, 0);
			var vector2 = new Vector2F(3, 4);

			Assert.True(vector1.InRange(vector2, 5));
			Assert.False(vector1.InRange(vector2, 4.9f));

			Assert.True(vector1.InRange(vector1, 0));

			var vector3 = new Vector2F(10, 0);
			Assert.True(vector1.InRange(vector3, 10));
			Assert.False(vector1.InRange(vector3, 9));
		}
	}
}
