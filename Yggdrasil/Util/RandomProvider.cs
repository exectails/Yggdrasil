// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;

namespace Yggdrasil.Util
{
	/// <summary>
	/// Thread-safe provider for "Random" instances.
	/// </summary>
	public static class RandomProvider
	{
		private static readonly Random _seed = new Random();
		private static readonly Random _rnd = new Random(_seed.Next());

		[ThreadStatic]
		private static Random _random;

		/// <summary>
		/// Returns an instance of Random for the calling thread.
		/// </summary>
		/// <returns></returns>
		public static Random Get()
		{
			if (_random != null)
				return _random;

			var seed = GetSeed();

			return (_random = new Random(seed));
		}

		/// <summary>
		/// Returns a random seed.
		/// </summary>
		/// <returns></returns>
		public static int GetSeed()
		{
			lock (_seed)
				return _seed.Next();
		}

		/// <summary>
		/// Returns a random integer.
		/// </summary>
		/// <returns></returns>
		public static int Next()
		{
			lock (_rnd)
				return _rnd.Next();
		}

		/// <summary>
		/// Returns a random integer between 0 and max - 1.
		/// </summary>
		/// <param name="max"></param>
		/// <returns></returns>
		public static int Next(int max)
		{
			lock (_rnd)
				return _rnd.Next(max);
		}

		/// <summary>
		/// Returns a random integer between min and max - 1.
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public static int Next(int min, int max)
		{
			lock (_rnd)
				return _rnd.Next(min, max);
		}

		/// <summary>
		/// Returns a random double between 0.0 and 1.0.
		/// </summary>
		/// <returns></returns>
		public static double NextDouble()
		{
			lock (_rnd)
				return _rnd.NextDouble();
		}
	}
}
