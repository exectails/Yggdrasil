using System;
using System.Threading;
using Xunit;
using Yggdrasil.Util;

namespace Yggdrasil.Test.Util
{
	public class RandomProviderTests
	{
		/// <summary>
		/// Tests that the same thread gets the same Random object on
		/// repeating calls to Get, and that two threads don't get the same
		/// object.
		/// </summary>
		[Fact]
		public void Get()
		{
			Random rnd1 = null, rnd2 = null;
			Random rnd3 = null, rnd4 = null;

			var thread1 = new Thread(() =>
			{
				rnd1 = RandomProvider.Get();
				rnd3 = RandomProvider.Get();
			});

			var thread2 = new Thread(() =>
			{
				rnd2 = RandomProvider.Get();
				rnd4 = RandomProvider.Get();
			});

			thread1.Start();
			thread2.Start();

			thread1.Join();
			thread2.Join();

			Assert.NotEqual(rnd1, rnd2);
			Assert.Equal(rnd1, rnd3);
			Assert.Equal(rnd2, rnd4);
		}
	}
}
