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
			Assert.Equal(100f, "test".GetSimilarity("test"), 2f);

			Assert.Equal(0, "lorem ipsum".GetLevenshteinDistance("lorem ipsum"));
			Assert.Equal(100f, "lorem ipsum".GetSimilarity("lorem ipsum"), 2f);

			Assert.Equal(9, "test".GetLevenshteinDistance("lorem ipsum"));
			Assert.Equal(18.18, "test".GetSimilarity("lorem ipsum"), 2);

			Assert.Equal(6, "foobar".GetLevenshteinDistance("barfoo"));
			Assert.Equal(0f, "foobar".GetSimilarity("barfoo"), 2f);

			Assert.Equal(3, "foobar".GetLevenshteinDistance("foofoo"));
			Assert.Equal(50f, "foobar".GetSimilarity("foofoo"), 2f);

			Assert.Equal(1, "asdf".GetLevenshteinDistance("asdb"));
			Assert.Equal(75f, "asdf".GetSimilarity("asdb"), 2f);

			Assert.Equal(3, "asdf".GetLevenshteinDistance("adbc"));
			Assert.Equal(25f, "asdf".GetSimilarity("adbc"), 2f);
		}

		[Fact]
		public void IsNullOrWhiteSpace()
		{
			Assert.True(((string)null).IsNullOrWhiteSpace());
			Assert.True("".IsNullOrWhiteSpace());
			Assert.True(" ".IsNullOrWhiteSpace());
			Assert.True("\t".IsNullOrWhiteSpace());
			Assert.True("\n".IsNullOrWhiteSpace());
			Assert.True("\r".IsNullOrWhiteSpace());
			Assert.True("    ".IsNullOrWhiteSpace());
			Assert.False("  a  ".IsNullOrWhiteSpace());
			Assert.False("abc".IsNullOrWhiteSpace());
		}
	}
}
