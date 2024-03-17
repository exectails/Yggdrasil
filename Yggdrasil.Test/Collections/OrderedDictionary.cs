using System;
using System.Collections.Generic;
using Xunit;
using Yggdrasil.Collections;

namespace Yggdrasil.Test.Collections
{
	public class OrderedDictionaryTests
	{
		[Fact]
		public void IntKeying()
		{
			Assert.Throws<NotSupportedException>(() => { new OrderedDictionary<int, int>(); });
		}

		[Fact]
		public void AddingAndGetting()
		{
			var dict = new OrderedDictionary<string, int>();

			dict.Add("1", 1000);
			dict.Add("2", 2000);
			dict.Add("3", 3000);

			Assert.Equal(3, dict.Count);
			Assert.Equal(1000, dict["1"]);
			Assert.Equal(2000, dict["2"]);
			Assert.Equal(3000, dict["3"]);
			Assert.Throws<KeyNotFoundException>(() => { Assert.Equal(4000, dict["4"]); });

			Assert.Equal(1000, dict[0]);
			Assert.Equal(2000, dict[1]);
			Assert.Equal(3000, dict[2]);
			Assert.Throws<ArgumentOutOfRangeException>(() => { Assert.Equal(4000, dict[3]); });
		}

		[Fact]
		public void Order()
		{
			var dict = new OrderedDictionary<string, int>();

			dict.Add("1", 1000);
			dict.Add("2", 2000);
			dict.Add("3", 3000);
			dict.Add("4", 4000);
			dict.Add("5", 5000);

			dict.Remove("3");
			dict.Remove("4");
			dict.Add("3", 3300);
			dict.Add("4", 4400);

			dict.Remove("1");
			dict.Add("1", 1100);

			Assert.Equal(5, dict.Count);
			Assert.Equal(2000, dict[0]);
			Assert.Equal(5000, dict[1]);
			Assert.Equal(3300, dict[2]);
			Assert.Equal(4400, dict[3]);
			Assert.Equal(1100, dict[4]);
		}

		[Fact]
		public void IndexOf()
		{
			var dict = new OrderedDictionary<string, int>();

			dict.Add("1", 1000);
			dict.Add("2", 2000);
			dict.Add("3", 3000);
			dict.Add("4", 4000);
			dict.Add("5", 5000);

			dict.Remove("3");
			dict.Remove("4");
			dict.Add("3", 3300);
			dict.Add("4", 4400);

			dict.Remove("1");
			dict.Add("1", 1100);

			Assert.Equal(5, dict.Count);
			Assert.Equal(3, dict.IndexOf("4"));
			Assert.Equal(-1, dict.IndexOf("6"));

			dict.Insert(0, "6", 6000);
			Assert.Equal(6, dict.Count);
			Assert.Equal(4, dict.IndexOf("4"));
			Assert.Equal(0, dict.IndexOf("6"));
		}

		[Fact]
		public void RemoveAt()
		{
			var dict = new OrderedDictionary<string, int>();

			dict.Add("1", 1000);
			dict.Add("2", 2000);
			dict.Add("3", 3000);
			dict.Add("4", 4000);
			dict.Add("5", 5000);

			dict.Remove("3");
			dict.Remove("4");
			dict.Add("3", 3300);
			dict.Add("4", 4400);

			dict.Remove("1");
			dict.Add("1", 1100);

			dict.RemoveAt(1);

			Assert.Equal(4, dict.Count);
			Assert.Equal(2, dict.IndexOf("4"));
			Assert.Equal(-1, dict.IndexOf("5"));
		}
	}
}
