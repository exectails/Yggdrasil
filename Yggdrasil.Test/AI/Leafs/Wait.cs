// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Threading;
using Xunit;
using Yggrasil.Ai;
using Yggrasil.Ai.Composites;
using Yggrasil.Ai.Leafs;

namespace Yggdrasil.Test.AI.Leafs
{
	public class WaitTests
	{
		[Fact]
		public void Wait()
		{
			var state = new State();
			var waitTime = 2000;

			var routine = new Wait(TimeSpan.FromMilliseconds(waitTime));

			Assert.Equal(RoutineStatus.Running, routine.Act(state));
			Thread.Sleep(1000);
			Assert.Equal(RoutineStatus.Running, routine.Act(state));
			Thread.Sleep(1100);
			Assert.Equal(RoutineStatus.Success, routine.Act(state));
		}
	}
}
