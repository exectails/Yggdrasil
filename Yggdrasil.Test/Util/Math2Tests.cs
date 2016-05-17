// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Xunit;
using Yggdrasil.Util;

namespace Yggdrasil.Test.Util
{
	public class Math2Tests
	{
		[Fact]
		public void Clamp()
		{
			Assert.Equal(10, Math2.Clamp(10, 20, 05));
			Assert.Equal(20, Math2.Clamp(10, 20, 25));
			Assert.Equal(15, Math2.Clamp(10, 20, 15));

			Assert.Equal(10.5f, Math2.Clamp(10.5f, 20.5f, 05.5f));
			Assert.Equal(20.5f, Math2.Clamp(10.5f, 20.5f, 25.5f));
			Assert.Equal(15.5f, Math2.Clamp(10.5f, 20.5f, 15.5f));
		}

		[Fact]
		public void IsBetween()
		{
			Assert.True(Math2.IsBetween(10, 0, 20));
			Assert.True(Math2.IsBetween(0, -1, 1));
			Assert.True(Math2.IsBetween(short.MaxValue, 0, int.MaxValue));
		}
	}
}
