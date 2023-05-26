using System;
using Xunit;
using Yggdrasil.Extensions;

namespace Yggdrasil.Test.Extensions
{
	public class TimeSpanTests
	{
		[Fact]
		public void Divide()
		{
			var ts = TimeSpan.FromSeconds(10);

			Assert.Equal(TimeSpan.FromSeconds(5), ts.Divide(2));
			Assert.Equal(TimeSpan.FromSeconds(2), ts.Divide(5));
			Assert.Equal(TimeSpan.FromSeconds(1), ts.Divide(10));

			Assert.Equal(TimeSpan.FromMilliseconds(5000), ts.Divide(2));
			Assert.Equal(TimeSpan.FromMilliseconds(2000), ts.Divide(5));
			Assert.Equal(TimeSpan.FromMilliseconds(1000), ts.Divide(10));
		}

		[Fact]
		public void Multiply()
		{
			var ts = TimeSpan.FromSeconds(10);

			Assert.Equal(TimeSpan.FromSeconds(20), ts.Multiply(2));
			Assert.Equal(TimeSpan.FromSeconds(50), ts.Multiply(5));
			Assert.Equal(TimeSpan.FromSeconds(100), ts.Multiply(10));

			Assert.Equal(TimeSpan.FromMilliseconds(20000), ts.Multiply(2));
			Assert.Equal(TimeSpan.FromMilliseconds(50000), ts.Multiply(5));
			Assert.Equal(TimeSpan.FromMilliseconds(100000), ts.Multiply(10));

			Assert.Equal(TimeSpan.FromMinutes(1), ts.Multiply(6));
			Assert.Equal(TimeSpan.FromMinutes(5), ts.Multiply(30));
		}
	}
}
