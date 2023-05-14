using System;
using System.Net;
using Yggdrasil.Util;

namespace Yggdrasil.Extensions
{
	/// <summary>
	/// Extensions for IPAddress.
	/// </summary>
	public static class IpAddressExtensions
	{
		/// <summary>
		/// Returns IP address as integer.
		/// </summary>
		/// <example>
		/// IPAddress.Parse("127.0.0.1").ToInt32(); // 0x0100007F
		/// IPAddress.Parse("127.0.0.1", Endianness.BigEndian).ToInt32(); // 0x7F000001
		/// </example>
		/// <param name="ipAddress"></param>
		/// <param name="endianness"></param>
		/// <returns></returns>
		public static int ToInt32(this IPAddress ipAddress, Endianness endianness = Endianness.LittleEndian)
		{
			var bytes = ipAddress.GetAddressBytes();

			if (endianness == Endianness.BigEndian)
				Array.Reverse(bytes);

			return BitConverter.ToInt32(bytes, 0);
		}
	}
}
