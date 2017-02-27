// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;

namespace Yggdrasil.Util
{
	/// <summary>
	/// Mersenne Twister Random Number Generator.
	/// </summary>
	public class MT19937
	{
		private const int N = 624;
		private const int M = 397;
		private const uint UpperMask = 0x80000000;
		private const uint LowerMask = 0x7FFFFFFF;
		private readonly uint[] Matrix = new uint[] { 0x00, 0x9908B0DF };

		private int index = N;
		private uint[] state = new uint[N];

		/// <summary>
		/// Creates new instance of generator, with the current tick count
		/// as seed.
		/// </summary>
		public MT19937()
			: this((uint)Environment.TickCount)
		{
		}

		/// <summary>
		/// Creates new instance of generator, with the given seed.
		/// </summary>
		/// <param name="seed"></param>
		public MT19937(uint seed)
		{
			this.Init(seed);
		}

		/// <summary>
		/// (Re-)initiates the generator with the given seed.
		/// </summary>
		/// <param name="seed"></param>
		public void Init(uint seed)
		{
			index = N;
			state[0] = seed & 0xFFFFFFFF;
			for (int i = 1; i < N; ++i)
				state[i] = (0x00010DCD * state[i - 1]) & 0xFFFFFFFF;
		}

		/// <summary>
		/// Generates a random uint.
		/// </summary>
		/// <returns></returns>
		private uint Round()
		{
			if (index >= N)
			{
				uint j;

				for (int i = 0; i < N - M; i++)
				{
					j = (state[i] & UpperMask) | (state[i + 1] & LowerMask);
					state[i] = state[i + M] ^ (j >> 1) ^ Matrix[j & 0x1];
				}

				for (int i = N - M; i < N - 1; i++)
				{
					j = (state[i] & UpperMask) | (state[i + 1] & LowerMask);
					state[i] = state[i + (M - N)] ^ (j >> 1) ^ Matrix[j & 0x1];
				}

				j = (state[N - 1] & UpperMask) | (state[0] & LowerMask);
				state[N - 1] = state[M - 1] ^ (j >> 1) ^ Matrix[j & 0x1];

				index = 0;
			}

			var result = state[index++];
			result ^= (result >> 0x0B);
			result ^= (result << 0x07) & 0x9D2C5680;
			result ^= (result << 0x0F) & 0xEFC60000;
			result ^= (result >> 0x12);

			return result;
		}

		/// <summary>
		/// Returns a random double between 0.0 and 1.0.
		/// </summary>
		/// <returns></returns>
		public double NextDouble()
		{
			return ((double)this.Round() / (double)uint.MaxValue);
		}

		/// <summary>
		/// Returns a random integer.
		/// </summary>
		/// <returns></returns>
		public int Next()
		{
			return (int)this.Round();
		}

		/// <summary>
		/// Returns a random integer between min (inclusive) and max (exclusive).
		/// </summary>
		/// <param name="max"></param>
		/// <returns></returns>
		public int Next(int max)
		{
			return (int)(this.Round() / (double)uint.MaxValue * max);
		}

		/// <summary>
		/// Returns a random integer between min (inclusive) and max (exclusive).
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public int Next(int min, int max)
		{
			return min + (int)(this.Round() / (double)uint.MaxValue * (max - min));
		}

		/// <summary>
		/// Returns a random long integer.
		/// </summary>
		/// <returns></returns>
		public long NextInt64()
		{
			var n1 = (long)this.Next();
			var n2 = (long)this.Next();

			return (n1 << (8 * sizeof(int))) + n2;
		}
	}
}
