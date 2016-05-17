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
	}
}
