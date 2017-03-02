// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Text;

namespace Yggdrasil.Security.Hashing
{
	/// <summary>
	/// Quick access to an SHA256 encoder.
	/// </summary>
	public static class SHA256
	{
		private readonly static System.Security.Cryptography.SHA256 _encoder = System.Security.Cryptography.SHA256.Create();

		/// <summary>
		/// Returns encoded value.
		/// </summary>
		/// <param name="val">Array to hash.</param>
		/// <returns></returns>
		public static byte[] Encode(byte[] val)
		{
			lock (_encoder)
				return _encoder.ComputeHash(val);
		}

		/// <summary>
		/// Returns encoded value.
		/// </summary>
		/// <param name="val">Value to hash.</param>
		/// <returns></returns>
		public static string Encode(string val)
		{
			var bytes = Encoding.UTF8.GetBytes(val);

			lock (_encoder)
				bytes = _encoder.ComputeHash(bytes);

			return BitConverter.ToString(bytes).Replace("-", "").ToUpper();
		}
	}
}
