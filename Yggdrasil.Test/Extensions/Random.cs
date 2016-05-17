// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;
using Xunit;
using Yggdrasil.Extensions;
using Yggdrasil.Util;

namespace Yggdrasil.Test.Extensions
{
	public class RandomExtensionTests
	{
		[Fact]
		public void RandomElementFromIList()
		{
			var list = new List<int>() { 11, 23, 34, 42 };

			for (int i = 0; i < 1000000; ++i)
				Assert.Contains(list.Random(), list);
		}

		[Fact]
		public void Between()
		{
			var rnd = RandomProvider.Get();

			for (int i = 0; i < 1000000; ++i)
				Assert.InRange(rnd.Between(10, 20), 10, 20);
		}

		[Fact]
		public void Rnd()
		{
			var list = new int[] { 101, 203, 305, 407 };
			var rnd = RandomProvider.Get();

			for (int i = 0; i < 1000000; ++i)
				Assert.Contains(rnd.Rnd(list), list);
		}
	}
}
