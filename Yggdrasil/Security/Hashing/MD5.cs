// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Text;

namespace Yggdrasil.Security.Hashing
{
	/// <summary>
	/// Quick access to an MD5 encoder.
	/// </summary>
	public static class MD5
	{
		private readonly static System.Security.Cryptography.MD5 _md5 = System.Security.Cryptography.MD5.Create();

		/// <summary>
		/// Returns encoded value.
		/// </summary>
		/// <param name="val">Array to hash.</param>
		/// <returns></returns>
		public static byte[] Encode(byte[] val)
		{
			lock (_md5)
				return _md5.ComputeHash(val);
		}

		/// <summary>
		/// Returns encoded value.
		/// </summary>
		/// <param name="val">Value to hash.</param>
		/// <returns></returns>
		public static string Encode(string val)
		{
			var bytes = Encoding.UTF8.GetBytes(val);

			lock (_md5)
				bytes = _md5.ComputeHash(bytes);

			return BitConverter.ToString(bytes).Replace("-", "").ToUpper();
		}
	}
}
