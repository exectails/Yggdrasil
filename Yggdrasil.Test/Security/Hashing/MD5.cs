using System.Text;
using Xunit;
using Yggdrasil.Security.Hashing;

namespace Yggdrasil.Test.Security.Hashing
{
	public class MD5Tests
	{
		[Fact]
		public void Encode()
		{
			Assert.Equal("81DC9BDB52D04DC20036DBD8313ED055", MD5.Encode("1234"));
			Assert.Equal(new byte[] { 0x81, 0xDC, 0x9B, 0xDB, 0x52, 0xD0, 0x4D, 0xC2, 0x00, 0x36, 0xDB, 0xD8, 0x31, 0x3E, 0xD0, 0x55 }, MD5.Encode(Encoding.UTF8.GetBytes("1234")));

			Assert.Equal("65A8E27D8879283831B664BD8B7F0AD4", MD5.Encode("Hello, World!"));
			Assert.Equal(new byte[] { 0x65, 0xA8, 0xE2, 0x7D, 0x88, 0x79, 0x28, 0x38, 0x31, 0xB6, 0x64, 0xBD, 0x8B, 0x7F, 0x0A, 0xD4 }, MD5.Encode(Encoding.UTF8.GetBytes("Hello, World!")));
		}
	}
}
