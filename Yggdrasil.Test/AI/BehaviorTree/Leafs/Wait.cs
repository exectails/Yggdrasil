// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Threading;
using Xunit;
using Yggrasil.Ai.BehaviorTree;
using Yggrasil.Ai.BehaviorTree.Composites;
using Yggrasil.Ai.BehaviorTree.Leafs;

namespace Yggdrasil.Test.AI.BehaviorTree.Leafs
{
	public class WaitTests
	{
		[Fact]
		public void Wait_2000()
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

		[Fact]
		public void Wait_0()
		{
			var state = new State();
			var waitTime = 0;

			var routine = new Wait(TimeSpan.FromMilliseconds(waitTime));

			Assert.Equal(RoutineStatus.Success, routine.Act(state));
			Thread.Sleep(1000);
			Assert.Equal(RoutineStatus.Success, routine.Act(state));
		}
	}
}
