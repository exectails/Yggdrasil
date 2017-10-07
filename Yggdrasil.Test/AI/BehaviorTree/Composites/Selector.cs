// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Xunit;
using Yggrasil.Ai.BehaviorTree;
using Yggrasil.Ai.BehaviorTree.Composites;

namespace Yggdrasil.Test.AI.BehaviorTree.Composites
{
	public class SelectorTests
	{
		[Fact]
		public void Selector_RunTillSuccess()
		{
			var state = new State();
			var test = 0;

			var selector = new Selector(
				new Conditional(_ => { test++; return false; }),
				new Conditional(_ => { test++; return false; }),
				new Conditional(_ => { test++; return false; }),
				new Conditional(_ => { test++; return true; }),
				new Conditional(_ => { test++; return false; })
			);

			Assert.Equal(RoutineStatus.Running, selector.Act(state));
			Assert.Equal(1, test);
			Assert.Equal(RoutineStatus.Running, selector.Act(state));
			Assert.Equal(2, test);
			Assert.Equal(RoutineStatus.Running, selector.Act(state));
			Assert.Equal(3, test);
			Assert.Equal(RoutineStatus.Success, selector.Act(state));
			Assert.Equal(4, test);
			Assert.Equal(RoutineStatus.Success, selector.Act(state));
			Assert.Equal(4, test);
		}

		[Fact]
		public void Selector_RunTillEnd()
		{
			var state = new State();
			var test = 0;

			var selector = new Selector(
				new Conditional(_ => { test++; return false; }),
				new Conditional(_ => { test++; return false; }),
				new Conditional(_ => { test++; return false; }),
				new Conditional(_ => { test++; return false; }),
				new Conditional(_ => { test++; return false; })
			);

			Assert.Equal(RoutineStatus.Running, selector.Act(state));
			Assert.Equal(1, test);
			Assert.Equal(RoutineStatus.Running, selector.Act(state));
			Assert.Equal(2, test);
			Assert.Equal(RoutineStatus.Running, selector.Act(state));
			Assert.Equal(3, test);
			Assert.Equal(RoutineStatus.Running, selector.Act(state));
			Assert.Equal(4, test);
			Assert.Equal(RoutineStatus.Failure, selector.Act(state));
			Assert.Equal(5, test);
			Assert.Equal(RoutineStatus.Failure, selector.Act(state));
			Assert.Equal(5, test);
		}
	}
}
