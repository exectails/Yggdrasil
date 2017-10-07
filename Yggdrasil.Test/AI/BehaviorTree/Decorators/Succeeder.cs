// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Xunit;
using Yggrasil.Ai.BehaviorTree;
using Yggrasil.Ai.BehaviorTree.Composites;
using Yggrasil.Ai.BehaviorTree.Decorators;

namespace Yggdrasil.Test.AI.BehaviorTree.Decorators
{
	public class SucceederTests
	{
		[Fact]
		public void Succeeder()
		{
			var state = new State();
			var test = 0;

			var routine = new Succeeder(new Conditional((_) => test < 50));

			for (; test < 50; ++test)
			{
				Assert.Equal(RoutineStatus.Success, routine.Act(state));
				state.Reset();
			}

			test++;
			Assert.Equal(RoutineStatus.Success, routine.Act(state));
			state.Reset();

			Assert.Equal(RoutineStatus.Success, routine.Act(state));
			state.Reset();

			test = 1;
			Assert.Equal(RoutineStatus.Success, routine.Act(state));
			state.Reset();
		}
	}
}
