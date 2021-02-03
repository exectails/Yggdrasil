using System;
using Xunit;
using Yggdrasil.Util;

namespace Yggdrasil.Test.Util
{
	public class Math2Tests
	{
		[Fact]
		public void Min()
		{
			Assert.Equal(TimeSpan.FromSeconds(1), Math2.Min(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)));
			Assert.Equal(TimeSpan.FromSeconds(2), Math2.Min(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(2)));
			Assert.Equal(TimeSpan.FromSeconds(2), Math2.Min(TimeSpan.FromHours(1), TimeSpan.FromSeconds(2)));
		}

		[Fact]
		public void Max()
		{
			Assert.Equal(TimeSpan.FromSeconds(2), Math2.Max(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)));
			Assert.Equal(TimeSpan.FromSeconds(3), Math2.Max(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(2)));
			Assert.Equal(TimeSpan.FromHours(1), Math2.Max(TimeSpan.FromHours(1), TimeSpan.FromSeconds(2)));
		}

		[Fact]
		public void Clamp()
		{
			Assert.Equal(10, Math2.Clamp(10, 20, 05));
			Assert.Equal(20, Math2.Clamp(10, 20, 25));
			Assert.Equal(15, Math2.Clamp(10, 20, 15));

			Assert.Equal(10.5f, Math2.Clamp(10.5f, 20.5f, 05.5f));
			Assert.Equal(20.5f, Math2.Clamp(10.5f, 20.5f, 25.5f));
			Assert.Equal(15.5f, Math2.Clamp(10.5f, 20.5f, 15.5f));

			Assert.Equal(TimeSpan.FromSeconds(5), Math2.Clamp(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(5)));
			Assert.Equal(TimeSpan.FromSeconds(3), Math2.Clamp(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(5)));
			Assert.Equal(TimeSpan.FromSeconds(2), Math2.Clamp(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(1)));
		}

		[Fact]
		public void IsBetween()
		{
			Assert.True(Math2.IsBetween(10, 0, 20));
			Assert.True(Math2.IsBetween(0, -1, 1));
			Assert.True(Math2.IsBetween(short.MaxValue, 0, int.MaxValue));
		}

		[Fact]
		public void MultiplyCheckedShort()
		{
			// Positive
			Assert.Equal(10000, Math2.MultiplyChecked((short)5000, 2));
			Assert.Equal(short.MaxValue, Math2.MultiplyChecked((short)20000, 2));
			Assert.Equal(short.MaxValue, Math2.MultiplyChecked((short)16000, 3));

			// Negative
			Assert.Equal(-10000, Math2.MultiplyChecked((short)-5000, 2));
			Assert.Equal(short.MinValue, Math2.MultiplyChecked((short)-20000, 2));
			Assert.Equal(short.MinValue, Math2.MultiplyChecked((short)-16000, 3));
		}

		[Fact]
		public void MultiplyCheckedInt()
		{
			// Positive
			Assert.Equal(100000, Math2.MultiplyChecked(50000, 2));
			Assert.Equal(2000000000, Math2.MultiplyChecked(1000000000, 2));
			Assert.Equal(int.MaxValue, Math2.MultiplyChecked(2000000000, 2));
			Assert.Equal(int.MaxValue, Math2.MultiplyChecked(1000000000, 3));

			// Negative
			Assert.Equal(-100000, Math2.MultiplyChecked(-50000, 2));
			Assert.Equal(-2000000000, Math2.MultiplyChecked(-1000000000, 2));
			Assert.Equal(int.MinValue, Math2.MultiplyChecked(-2000000000, 2));
			Assert.Equal(int.MinValue, Math2.MultiplyChecked(-1000000000, 3));
		}

		[Fact]
		public void MultiplyCheckedLong()
		{
			// Positive
			Assert.Equal(100000L, Math2.MultiplyChecked(50000L, 2));
			Assert.Equal(2000000000L, Math2.MultiplyChecked(1000000000L, 2));
			Assert.Equal(4000000000L, Math2.MultiplyChecked(2000000000L, 2));
			Assert.Equal(long.MaxValue, Math2.MultiplyChecked(5000000000000000000, 2));
			Assert.Equal(long.MaxValue, Math2.MultiplyChecked(4500000000000000000, 3));

			// Negative
			Assert.Equal(-100000L, Math2.MultiplyChecked(-50000, 2));
			Assert.Equal(-2000000000L, Math2.MultiplyChecked(-1000000000L, 2));
			Assert.Equal(-4000000000L, Math2.MultiplyChecked(-2000000000L, 2));
			Assert.Equal(long.MinValue, Math2.MultiplyChecked(-5000000000000000000, 2));
			Assert.Equal(long.MinValue, Math2.MultiplyChecked(-4500000000000000000, 3));
		}

		[Fact]
		public void DegreeToRadian()
		{
			Assert.Equal(0, Math2.DegreeToRadian(0));
			Assert.Equal((float)Math.PI / 4f, Math2.DegreeToRadian(45));
			Assert.Equal(-(float)Math.PI / 4f, Math2.DegreeToRadian(-45));
			Assert.Equal((float)Math.PI / 2f, Math2.DegreeToRadian(90));
			Assert.Equal((float)Math.PI, Math2.DegreeToRadian(180));
			Assert.Equal((float)Math.PI * 1.5f, Math2.DegreeToRadian(270));
			Assert.Equal((float)Math.PI * 2f, Math2.DegreeToRadian(360));
			Assert.Equal(-(float)Math.PI * 2f, Math2.DegreeToRadian(-360));
		}

		[Fact]
		public void RadianToDegree()
		{
			Assert.Equal(0, Math2.RadianToDegree(0));
			Assert.Equal(45, Math2.RadianToDegree((float)Math.PI / 4f));
			Assert.Equal(-45, Math2.RadianToDegree(-(float)Math.PI / 4f));
			Assert.Equal(90, Math2.RadianToDegree((float)Math.PI / 2f));
			Assert.Equal(180, Math2.RadianToDegree((float)Math.PI));
			Assert.Equal(270, Math2.RadianToDegree((float)Math.PI * 1.5f));
			Assert.Equal(360, Math2.RadianToDegree((float)Math.PI * 2f));
			Assert.Equal(-360, Math2.RadianToDegree(-(float)Math.PI * 2f));
		}
	}
}
