using Xunit;
using Yggdrasil.Extensions;

namespace Yggdrasil.Test.Extensions
{
	public class StringExtensionTests
	{
		[Fact]
		public void LevenshteinDistance()
		{
			Assert.Equal(0, "test1".GetLevenshteinDistance("test1"));
			Assert.Equal(1, "test2".GetLevenshteinDistance("test3"));
			Assert.Equal(4, "test4".GetLevenshteinDistance("test4 asd"));
			Assert.Equal(3, "kitten".GetLevenshteinDistance("sitting"));
		}

		[Fact]
		public void Similarity()
		{
			Assert.Equal(0, "test".GetLevenshteinDistance("test"));
			Assert.Equal(100, "test".GetSimilarity("test"), 2);

			Assert.Equal(0, "lorem ipsum".GetLevenshteinDistance("lorem ipsum"));
			Assert.Equal(100, "lorem ipsum".GetSimilarity("lorem ipsum"), 2);

			Assert.Equal(9, "test".GetLevenshteinDistance("lorem ipsum"));
			Assert.Equal(18.18, "test".GetSimilarity("lorem ipsum"), 2);

			Assert.Equal(6, "foobar".GetLevenshteinDistance("barfoo"));
			Assert.Equal(0, "foobar".GetSimilarity("barfoo"), 2);

			Assert.Equal(3, "foobar".GetLevenshteinDistance("foofoo"));
			Assert.Equal(50, "foobar".GetSimilarity("foofoo"), 2);

			Assert.Equal(1, "asdf".GetLevenshteinDistance("asdb"));
			Assert.Equal(75, "asdf".GetSimilarity("asdb"), 2);

			Assert.Equal(3, "asdf".GetLevenshteinDistance("adbc"));
			Assert.Equal(25, "asdf".GetSimilarity("adbc"), 2);
		}

		[Fact]
		public void IsNullOrWhiteSpace()
		{
			Assert.Equal(true, ((string)null).IsNullOrWhiteSpace());
			Assert.Equal(true, "".IsNullOrWhiteSpace());
			Assert.Equal(true, " ".IsNullOrWhiteSpace());
			Assert.Equal(true, "\t".IsNullOrWhiteSpace());
			Assert.Equal(true, "\n".IsNullOrWhiteSpace());
			Assert.Equal(true, "\r".IsNullOrWhiteSpace());
			Assert.Equal(true, "    ".IsNullOrWhiteSpace());
			Assert.Equal(false, "  a  ".IsNullOrWhiteSpace());
			Assert.Equal(false, "abc".IsNullOrWhiteSpace());
		}
	}
}
