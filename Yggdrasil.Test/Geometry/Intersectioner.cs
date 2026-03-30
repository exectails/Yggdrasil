using Xunit;
using Yggdrasil.Geometry;
using Yggdrasil.Geometry.Shapes;

namespace Yggdrasil.Test.Geometry
{
	public class IntersectionerTests
	{
		[Fact]
		public void CircleIntersectsCircle()
		{
			var c1 = new CircleF(new Vector2F(0, 0), 10);
			var c2 = new CircleF(new Vector2F(5, 0), 10);
			var c3 = new CircleF(new Vector2F(25, 0), 10);

			Assert.True(Intersectioner.Intersects(c1, c2));
			Assert.False(Intersectioner.Intersects(c1, c3));
		}

		[Fact]
		public void CircleIntersectsCapsule()
		{
			var circle = new CircleF(new Vector2F(0, 0), 10);
			var capsule1 = new CapsuleF(new Vector2F(15, 0), new Vector2F(25, 0), 10);
			var capsule2 = new CapsuleF(new Vector2F(25, 0), new Vector2F(35, 0), 2);

			Assert.True(Intersectioner.Intersects(circle, capsule1));
			Assert.False(Intersectioner.Intersects(circle, capsule2));
		}

		[Fact]
		public void CircleIntersectsCone()
		{
			var circle = new CircleF(new Vector2F(10, 0), 5);
			var cone1 = new ConeF(new Vector2F(0, 0), 0, 90, 20);
			var cone2 = new ConeF(new Vector2F(0, 0), 180, 90, 20);

			Assert.True(Intersectioner.Intersects(circle, cone1));
			Assert.False(Intersectioner.Intersects(circle, cone2));
		}

		[Fact]
		public void CircleIntersectsPolygon()
		{
			var circle = new CircleF(new Vector2F(0, 0), 5);
			var poly1 = new PolygonF(new[] { new Vector2F(-5, -5), new Vector2F(5, -5), new Vector2F(5, 5), new Vector2F(-5, 5) });
			var poly2 = new PolygonF(new[] { new Vector2F(15, -5), new Vector2F(25, -5), new Vector2F(25, 5), new Vector2F(15, 5) });

			Assert.True(Intersectioner.Intersects(circle, poly1));
			Assert.True(Intersectioner.Intersects(poly1, circle));
			Assert.False(Intersectioner.Intersects(circle, poly2));
			Assert.False(Intersectioner.Intersects(poly2, circle));
		}

		[Fact]
		public void CapsuleIntersectsPolygon()
		{
			var capsule = new CapsuleF(new Vector2F(-10, 0), new Vector2F(10, 0), 2);
			var poly1 = new PolygonF(new[] { new Vector2F(-5, -5), new Vector2F(5, -5), new Vector2F(5, 5), new Vector2F(-5, 5) });
			var poly2 = new PolygonF(new[] { new Vector2F(15, -5), new Vector2F(25, -5), new Vector2F(25, 5), new Vector2F(15, 5) });

			Assert.True(Intersectioner.Intersects(capsule, poly1));
			Assert.True(Intersectioner.Intersects(poly1, capsule));
			Assert.False(Intersectioner.Intersects(capsule, poly2));
			Assert.False(Intersectioner.Intersects(poly2, capsule));
		}

		[Fact]
		public void ShapesIntersect()
		{
			var poly1 = new PolygonF(new[] { new Vector2F(-5, -5), new Vector2F(5, -5), new Vector2F(5, 5), new Vector2F(-5, 5) });
			var poly2 = new PolygonF(new[] { new Vector2F(0, 0), new Vector2F(10, 0), new Vector2F(10, 10), new Vector2F(0, 10) });
			var poly3 = new PolygonF(new[] { new Vector2F(15, 15), new Vector2F(25, 15), new Vector2F(25, 25), new Vector2F(15, 25) });

			Assert.True(Intersectioner.Intersects(poly1, poly2));
			Assert.False(Intersectioner.Intersects(poly1, poly3));
		}

		[Fact]
		public void CircleIntersectsLine()
		{
			var circle = new CircleF(new Vector2F(0, 0), 5);
			var line1 = new LineF(new Vector2F(-10, 0), new Vector2F(10, 0));
			var line2 = new LineF(new Vector2F(-10, 10), new Vector2F(10, 10));

			Assert.True(Intersectioner.Intersects(circle, line1));
			Assert.False(Intersectioner.Intersects(circle, line2));
		}

		[Fact]
		public void CapsuleIntersectsLine()
		{
			var capsule = new CapsuleF(new Vector2F(-5, 0), new Vector2F(5, 0), 2);
			var line1 = new LineF(new Vector2F(0, -10), new Vector2F(0, 10));
			var line2 = new LineF(new Vector2F(0, 5), new Vector2F(0, 10));

			Assert.True(Intersectioner.Intersects(capsule, line1));
			Assert.False(Intersectioner.Intersects(capsule, line2));
		}

		[Fact]
		public void ConeIntersectsLine()
		{
			var cone = new ConeF(new Vector2F(0, 0), 0, 90, 20);
			var line1 = new LineF(new Vector2F(10, -10), new Vector2F(10, 10));
			var line2 = new LineF(new Vector2F(-10, -10), new Vector2F(-10, 10));

			Assert.True(Intersectioner.Intersects(cone, line1));
			Assert.False(Intersectioner.Intersects(cone, line2));
		}

		[Fact]
		public void PolygonIntersectsLine()
		{
			var poly = new PolygonF(new[] { new Vector2F(-5, -5), new Vector2F(5, -5), new Vector2F(5, 5), new Vector2F(-5, 5) });
			var line1 = new LineF(new Vector2F(-10, 0), new Vector2F(10, 0));
			var line2 = new LineF(new Vector2F(-10, 10), new Vector2F(10, 10));

			Assert.True(Intersectioner.Intersects(poly, line1));
			Assert.False(Intersectioner.Intersects(poly, line2));
		}
	}
}
