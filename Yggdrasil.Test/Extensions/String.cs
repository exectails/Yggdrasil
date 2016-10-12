// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Xunit;
using Yggdrasil.Extensions;

namespace Yggdrasil.Test.Extensions
{
	public class StringExtensionTests
	{
		[Fact]
		public void LevenshteinDistance()
		{
			Assert.Equal(0, "test1".LevenshteinDistance("test1"));
			Assert.Equal(1, "test2".LevenshteinDistance("test3"));
			Assert.Equal(4, "test4".LevenshteinDistance("test4 asd"));
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
