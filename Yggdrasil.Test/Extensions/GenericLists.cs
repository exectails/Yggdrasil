using System.Collections.Generic;
using System.Linq;
using Xunit;
using Yggdrasil.Extensions;

namespace Yggdrasil.Test.Extensions
{
	public class GenericListsExtensionTests
	{
		[Fact]
		public void RandomElementFromIList()
		{
			var list = (IList<int>)new List<int>() { 11, 23, 34, 42 };
			var results = new HashSet<int>();

			for (var i = 0; i < 1000000; ++i)
			{
				var n = list.Random();
				results.Add(n);

				Assert.Contains(n, list);
			}

			Assert.Equal(list, results.OrderBy(a => a));
		}

		[Fact]
		public void RandomElementFromIEnumerable()
		{
			var list1 = new List<int>() { 11, 23, 34, 42 };
			var list2 = new List<int>() { 11, 23, 34 };
			var enumerable = list1.Where(a => a <= 34);
			var results = new HashSet<int>();

			for (var i = 0; i < 1000000; ++i)
			{
				var n = enumerable.Random();
				results.Add(n);

				Assert.Contains(n, list2);
			}

			Assert.Equal(list2, results.OrderBy(a => a));
		}
	}
}
