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
			=> ToString(byteArray, 0, byteArray.Length, options);

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
			var result = new StringBuilder();

			var lineLength = length;
			var brokeLine = false;
			var line = 0;

			if ((options & HexStringOptions.EightNewLine) != 0)
				lineLength = 8;
			else if ((options & HexStringOptions.SixteenNewLine) != 0)
				lineLength = 16;

			if ((options & HexStringOptions.ColNumbers) != 0)
			{
				var spacerWidth = 47;

				if ((options & HexStringOptions.LineNumbers) != 0)
					spacerWidth += 11;

				if ((options & HexStringOptions.AsciiText) != 0)
					spacerWidth += 19;

				result.Append('-', spacerWidth);
				result.AppendLine();

				if ((options & HexStringOptions.LineNumbers) != 0)
					result.Append(' ', 11);

				for (var i = 0; i < lineLength; ++i)
					result.AppendFormat("{0:X2} ", i);

				result.AppendLine();
				result.Append('-', spacerWidth);
				result.AppendLine();
			}

			for (var i = start; i < start + length; ++i)
			{
				if ((i == 0 || brokeLine) && (options & HexStringOptions.LineNumbers) != 0)
					result.AppendFormat("{0:X8}   ", line++ * lineLength);

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
							brokeLine = true;
					}
					else if ((options & HexStringOptions.SixteenNewLine) != 0)
					{
						if (((i + 1) % 16) == 0)
							brokeLine = true;
					}

					if (brokeLine)
					{
						if ((options & HexStringOptions.AsciiText) != 0)
						{
							result.Append("   ");
							for (var j = lineLength - 1; j >= 0; --j)
							{
								var b = byteArray[i - j];
								var chr = (b > ' ' && b < '~' ? (char)b : '.');
								result.Append(chr);
							}
						}

						result.AppendLine();
					}
				}

				if (i == start + length - 1)
				{
					if ((options & HexStringOptions.AsciiText) != 0)
					{
						var remaining = 0;

						if ((options & HexStringOptions.EightNewLine) != 0)
							remaining = (start + length) % 8;
						else if ((options & HexStringOptions.SixteenNewLine) != 0)
							remaining = (start + length) % 16;

						if (remaining == 0)
							remaining = lineLength;

						for (var j = 0; j < lineLength - remaining; ++j)
						{
							result.Append("  ");

							if ((options & HexStringOptions.CommaSeparated) != 0)
								result.Append(",");

							if ((options & HexStringOptions.SpaceSeparated) != 0)
								result.Append(" ");

							if ((options & HexStringOptions.DashSeparated) != 0)
								result.Append("-");
						}

						result.Append("   ");
						for (var j = remaining - 1; j >= 0; --j)
						{
							var b = byteArray[i - j];
							var chr = (b > ' ' && b < '~' ? (char)b : '.');
							result.Append(chr);
						}
					}
				}
			}

			return result.ToString().TrimEnd();
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

		/// <summary>
		/// Adds ASCII representation before new lines.
		/// </summary>
		AsciiText = 0x40,

		/// <summary>
		/// Adds line numbers at the start of new lines.
		/// </summary>
		LineNumbers = 0x80,

		/// <summary>
		/// Adds column numbers on the first line.
		/// </summary>
		ColNumbers = 0x100,

		/// <summary>
		/// A combination of options that result in a view similar to
		/// that of a hex editor.
		/// </summary>
		HexEditorFull = SpaceSeparated | SixteenNewLine | AsciiText | LineNumbers | ColNumbers,

		/// <summary>
		/// A combination of options that result in a view similar to
		/// that of a hex editor.
		/// </summary>
		HexEditor = SpaceSeparated | SixteenNewLine | AsciiText | LineNumbers,
	}
}
