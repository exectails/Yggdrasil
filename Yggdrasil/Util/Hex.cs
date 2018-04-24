using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Yggdrasil.Util
{
	/// <summary>
	/// Utility functions to work with hex strings.
	/// </summary>
	public static class Hex
	{
		private static readonly Regex _invalidHexCharacters = new Regex(@"[^0-9a-f]|0x", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		/// <summary>
		/// Converts hex string to byte array, ignoring any invalid
		/// characters.
		/// </summary>
		/// <example>
		/// Hex.ToByteArray("00 01 02"); // = byte[] { 0x00, 0x01, 0x02 };
		/// Hex.ToByteArray("00-01-02"); // = byte[] { 0x00, 0x01, 0x02 };
		/// Hex.ToByteArray("0x00, 0x01, 0x02"); // = byte[] { 0x00, 0x01, 0x02 };
		/// </example>
		/// <param name="hexString"></param>
		/// <returns></returns>
		/// <exception cref="InvalidHexStringException">
		/// Thrown if string does not contain a multiple of 2 valid hex
		/// characters.
		/// </exception>
		public static byte[] ToByteArray(string hexString)
		{
			hexString = _invalidHexCharacters.Replace(hexString, "");
			hexString = Regex.Replace(hexString, @"\s|-|,|0x", "");

			var characterCount = hexString.Length;
			if ((characterCount % 2) != 0)
				throw new InvalidHexStringException("Hex strings must contain a multiple of 2 characters.");

			var result = new byte[characterCount / 2];

			for (var i = 0; i < characterCount; i += 2)
				result[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);

			return result;
		}

		/// <summary>
		/// Converts byte array to hex string.
		/// </summary>
		/// <example>
		/// Hex.ToString(new byte[] { 0x00, 0x01, 0x02 }); // = "000102"
		/// Hex.ToString(new byte[] { 0x00, 0x01, 0x02 }, HexStringOptions.DashSeparated); // = "00-01-02"
		/// Hex.ToString(new byte[] { 0x00, 0x01, 0x02 }, HexStringOptions.OXPrefixed | HexStringOptions.CommaSeparated | HexStringOptions.SpaceSeparated); // = "0x00, 0x01, 0x02"
		/// </example>
		/// <param name="byteArray"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static string ToString(byte[] byteArray, HexStringOptions options = HexStringOptions.SpaceSeparated)
		{
			return ToString(byteArray, 0, byteArray.Length, options);
		}

		/// <summary>
		/// Converts byte array to hex string.
		/// </summary>
		/// <example>
		/// Hex.ToString(new byte[] { 0x00, 0x01, 0x02 }); // = "000102"
		/// Hex.ToString(new byte[] { 0x00, 0x01, 0x02 }, HexStringOptions.DashSeparated); // = "00-01-02"
		/// Hex.ToString(new byte[] { 0x00, 0x01, 0x02 }, HexStringOptions.OXPrefixed | HexStringOptions.CommaSeparated | HexStringOptions.SpaceSeparated); // = "0x00, 0x01, 0x02"
		/// </example>
		/// <param name="byteArray"></param>
		/// <param name="start"></param>
		/// <param name="length"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public static string ToString(byte[] byteArray, int start, int length, HexStringOptions options = HexStringOptions.SpaceSeparated)
		{
			var result = new StringBuilder(byteArray.Length * 2);

			var brokeLine = false;

			for (var i = start; i < start + length; ++i)
			{
				if (i != 0 && !brokeLine)
				{
					if ((options & HexStringOptions.CommaSeparated) != 0)
						result.Append(",");

					if ((options & HexStringOptions.SpaceSeparated) != 0)
						result.Append(" ");

					if ((options & HexStringOptions.DashSeparated) != 0)
						result.Append("-");
				}

				brokeLine = false;

				if ((options & HexStringOptions.OXPrefix) != 0)
					result.Append("0x");

				if ((options & HexStringOptions.LowerCase) != 0)
					result.Append(byteArray[i].ToString("x2"));
				else
					result.Append(byteArray[i].ToString("X2"));

				if (i != 0 && i != byteArray.Length - 1)
				{
					if ((options & HexStringOptions.EightNewLine) != 0)
					{
						if (((i + 1) % 8) == 0)
						{
							Console.WriteLine(result.ToString().Replace(Environment.NewLine, "|"));
							result.AppendLine();
							brokeLine = true;
						}
					}
					else if ((options & HexStringOptions.SixteenNewLine) != 0)
					{
						if (((i + 1) % 16) == 0)
						{
							result.AppendLine();
							brokeLine = true;
						}
					}
				}
			}

			return result.ToString().Trim();
		}
	}

	/// <summary>
	/// Thrown if an invalid hex string is passed to hex utility.
	/// </summary>
	public class InvalidHexStringException : Exception
	{
		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="msg"></param>
		public InvalidHexStringException(string msg) : base(msg)
		{
		}
	}

	/// <summary>
	/// Styling options for hex strings created by hex utility.
	/// </summary>
	[Flags]
	public enum HexStringOptions
	{
		/// <summary>
		/// Use no special options.
		/// </summary>
		None = 0x00,

		/// <summary>
		/// Hex characters are prefixed with "0x".
		/// </summary>
		OXPrefix = 0x01,

		/// <summary>
		/// Hex characters are separated by spaces.
		/// </summary>
		SpaceSeparated = 0x02,

		/// <summary>
		/// Hex characters are separated by dashes.
		/// </summary>
		DashSeparated = 0x04,

		/// <summary>
		/// Hex characters are separated by commas.
		/// </summary>
		CommaSeparated = 0x08,

		/// <summary>
		/// The hex characters will use lower case.
		/// </summary>
		LowerCase = 0x10,

		/// <summary>
		/// Adds a new line after 8 hex characters.
		/// </summary>
		EightNewLine = 0x20,

		/// <summary>
		/// Adds a new line after 16 hex characters.
		/// </summary>
		SixteenNewLine = 0x40,
	}
}
