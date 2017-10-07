// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Xunit;
using Yggrasil.Ai.BehaviorTree;
using Yggrasil.Ai.BehaviorTree.Composites;
using Yggrasil.Ai.BehaviorTree.Leafs;

namespace Yggdrasil.Test.AI.BehaviorTree.Leafs
{
	public class ExecuteTests
	{
		[Fact]
		public void Execute()
		{
			var state = new State();
			var test = 0;

			var sequence = new Execute(_ => test++);

			for (int i = 1; i <= 100; ++i)
			{
				Assert.Equal(RoutineStatus.Success, sequence.Act(state));
				Assert.Equal(i, test);
				state.Reset();
			}
		}
	}
}
