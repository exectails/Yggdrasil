using System;
using Xunit;
using Yggdrasil.Util;

namespace Yggdrasil.Test.Util
{
	public class HexTests
	{
		[Fact]
		public void StringToByteArray()
		{
			var expected = new byte[] { 0x01, 0x02, 0x55, 0x66, 0xAA, 0xFF };

			Assert.Equal(expected, Hex.ToByteArray("01025566aaFF"));
			Assert.Equal(expected, Hex.ToByteArray("01 02 55 66 aa FF"));
			Assert.Equal(expected, Hex.ToByteArray("01-02-55-66-aa-FF"));
			Assert.Equal(expected, Hex.ToByteArray("0x01 0x02 0x55 0x66 0xaa 0xFF"));
			Assert.Equal(expected, Hex.ToByteArray("0x01, 0x02, 0x55, 0x66, 0xaa, 0xFF"));

			Assert.Throws<InvalidHexStringException>(() => Hex.ToByteArray("00 02 55 66 aa FF a"));
			Assert.Throws<InvalidHexStringException>(() => Hex.ToByteArray("012"));
		}

		[Fact]
		public void ByteArrayToString()
		{
			var byteArray = new byte[] { 0x00, 0x02, 0x55, 0x66, 0xAA, 0xFF };

			Assert.Equal("00025566AAFF", Hex.ToString(byteArray));
			Assert.Equal("00025566AAFF", Hex.ToString(byteArray, HexStringOptions.None));
			Assert.Equal("00 02 55 66 AA FF", Hex.ToString(byteArray, HexStringOptions.SpaceSeparated));
			Assert.Equal("00-02-55-66-AA-FF", Hex.ToString(byteArray, HexStringOptions.DashSeparated));
			Assert.Equal("00,02,55,66,AA,FF", Hex.ToString(byteArray, HexStringOptions.CommaSeparated));
			Assert.Equal("00025566aaff", Hex.ToString(byteArray, HexStringOptions.LowerCase));
			Assert.Equal("0x000x020x550x660xAA0xFF", Hex.ToString(byteArray, HexStringOptions.OXPrefix));

			Assert.Equal("0x00,0x02,0x55,0x66,0xAA,0xFF", Hex.ToString(byteArray, HexStringOptions.OXPrefix | HexStringOptions.CommaSeparated));
			Assert.Equal("0x00 0x02 0x55 0x66 0xAA 0xFF", Hex.ToString(byteArray, HexStringOptions.OXPrefix | HexStringOptions.SpaceSeparated));
			Assert.Equal("0x00, 0x02, 0x55, 0x66, 0xAA, 0xFF", Hex.ToString(byteArray, HexStringOptions.OXPrefix | HexStringOptions.CommaSeparated | HexStringOptions.SpaceSeparated));

			byteArray = new byte[] { 0x00, 0x02, 0x55, 0x66, 0xAA, 0xFF, 0x00, 0x02, 0x55, 0x66, 0xAA, 0xFF };
			Assert.Equal("00025566AAFF0002" + Environment.NewLine + "5566AAFF", Hex.ToString(byteArray, HexStringOptions.EightNewLine));

			byteArray = new byte[] { 0x00, 0x02, 0x55, 0x66, 0xAA, 0xFF, 0x00, 0x02, 0x55, 0x66, 0xAA, 0xFF, 0x00, 0x02, 0x55, 0x66, 0xAA, 0xFF, 0x00, 0x02, 0x55, 0x66, 0xAA, 0xFF };
			Assert.Equal("00025566AAFF0002" + Environment.NewLine + "5566AAFF00025566" + Environment.NewLine + "AAFF00025566AAFF", Hex.ToString(byteArray, HexStringOptions.EightNewLine));
			Assert.Equal("0x00 0x02 0x55 0x66 0xAA 0xFF 0x00 0x02" + Environment.NewLine + "0x55 0x66 0xAA 0xFF 0x00 0x02 0x55 0x66" + Environment.NewLine + "0xAA 0xFF 0x00 0x02 0x55 0x66 0xAA 0xFF", Hex.ToString(byteArray, HexStringOptions.EightNewLine | HexStringOptions.SpaceSeparated | HexStringOptions.OXPrefix));
			Assert.Equal("00025566AAFF00025566AAFF00025566" + Environment.NewLine + "AAFF00025566AAFF", Hex.ToString(byteArray, HexStringOptions.SixteenNewLine));
			Assert.Equal("00 02 55 66 AA FF 00 02 55 66 AA FF 00 02 55 66" + Environment.NewLine + "AA FF 00 02 55 66 AA FF", Hex.ToString(byteArray, HexStringOptions.SixteenNewLine | HexStringOptions.SpaceSeparated));

			byteArray = new byte[] { 0x00, 0x02, 0x55, 0x66, 0xAA, 0xFF, 0x00, 0x02, 0x55, 0x66, 0xAA, 0xFF, 0x00, 0x02, 0x55, 0x66, 0xAA, 0xFF, 0x00, 0x02, 0x55, 0x66, 0xAA, 0xFF, 0x00, 0x02, 0x55, 0x66, 0xAA, 0xFF, 0x00, 0x02, 0x55, 0x66, 0xAA, 0xFF };
			Assert.Equal("00025566AAFF00025566AAFF00025566" + Environment.NewLine + "AAFF00025566AAFF00025566AAFF0002" + Environment.NewLine + "5566AAFF", Hex.ToString(byteArray, HexStringOptions.SixteenNewLine));
			Assert.Equal("00 02 55 66 AA FF 00 02 55 66 AA FF 00 02 55 66" + Environment.NewLine + "AA FF 00 02 55 66 AA FF 00 02 55 66 AA FF 00 02" + Environment.NewLine + "55 66 AA FF", Hex.ToString(byteArray, HexStringOptions.SixteenNewLine | HexStringOptions.SpaceSeparated));
		}
	}
}
