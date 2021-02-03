using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Yggdrasil.Extensions;
using Yggdrasil.Util;

namespace Yggdrasil.Test.Extensions
{
	public class RandomExtensionTests
	{
		[Fact]
		public void Between()
		{
			var rnd = RandomProvider.Get();

			for (var i = 0; i < 1000000; ++i)
				Assert.InRange(rnd.Between(10, 20), 10, 20);

			for (var i = 0; i < 1000000; ++i)
				Assert.InRange(rnd.Between(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(20)), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(20));
		}

		[Fact]
		public void Rnd()
		{
			var list = new int[] { 101, 203, 305, 407 };
			var rnd = RandomProvider.Get();
			var results = new HashSet<int>();

			for (var i = 0; i < 1000000; ++i)
			{
				var n = rnd.Rnd(list);
				results.Add(n);

				Assert.Contains(n, list);
			}

			Assert.Equal(list, results.OrderBy(a => a));
		}
	}
}
