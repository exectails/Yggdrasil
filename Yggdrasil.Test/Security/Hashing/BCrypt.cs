using Xunit;
using Yggdrasil.Security.Hashing;

namespace Yggdrasil.Test.Security.Hashing
{
	public class BCryptTests
	{
		[Fact]
		public void HashingAndChecking()
		{
			var hash1 = BCrypt.HashPassword("123456", BCrypt.GenerateSalt(5));
			Assert.True(BCrypt.CheckPassword("123456", hash1));
			Assert.False(BCrypt.CheckPassword("123457", hash1));

			var hash2 = BCrypt.HashPassword("x+U-vZ_ZT:7r)G<7", BCrypt.GenerateSalt(8));
			Assert.True(BCrypt.CheckPassword("x+U-vZ_ZT:7r)G<7", hash2));
			Assert.False(BCrypt.CheckPassword("x+U-vY_ZT:7r)G<7", hash2));

			var hash3 = BCrypt.HashPassword("FD681AACFB947867F5859F0BB0B291A7", BCrypt.GenerateSalt(10));
			Assert.True(BCrypt.CheckPassword("FD681AACFB947867F5859F0BB0B291A7", hash3));
			Assert.False(BCrypt.CheckPassword("fd681aacfb947867f5859f0bb0b291a7", hash3));
		}
	}
}
