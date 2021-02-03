using System;
using Xunit;
using Yggrasil.Ai.BehaviorTree;
using Yggrasil.Ai.BehaviorTree.Composites;
using Yggrasil.Ai.BehaviorTree.Decorators;
using Yggrasil.Ai.BehaviorTree.Leafs;

namespace Yggdrasil.Test.AI.BehaviorTree.Leafs
{
	public class RepeaterTests
	{
		[Fact]
		public void Repeater_Times()
		{
			var state = new State();
			var test = 0;

			var routine = new Repeater(2, new Execute(_ => test++));

			for (var i = 1; i <= 100; ++i)
			{
				Assert.Equal(RoutineStatus.Running, routine.Act(state));
				Assert.Equal(RoutineStatus.Success, routine.Act(state));
				Assert.Equal(i * 2, test);
				state.Reset();
			}

			for (var i = 1; i <= 100; ++i)
			{
				Assert.Equal(RoutineStatus.Running, routine.Act(state));
				Assert.Equal(RoutineStatus.Success, routine.Act(state));
				state.Reset();
				Assert.Equal(RoutineStatus.Running, routine.Act(state));
				Assert.Equal(RoutineStatus.Success, routine.Act(state));
				Assert.Equal(200 + i * 4, test);
				state.Reset();
			}
		}

		[Fact]
		public void Repeater_Unlimited()
		{
			var state = new State();
			var test = 0;

			var routine = new Repeater(
				new Sequence(
					new Execute(_ => test++),
					new Wait(TimeSpan.FromTicks(0)),
					new Execute(_ => test++)
				)
			);

			for (var i = 1; i <= 1234; ++i)
			{
				Assert.Equal(RoutineStatus.Running, routine.Act(state));
				Assert.Equal(RoutineStatus.Running, routine.Act(state));
				Assert.Equal(RoutineStatus.Running, routine.Act(state));
				Assert.Equal(RoutineStatus.Running, routine.Act(state));
				Assert.Equal(RoutineStatus.Running, routine.Act(state));
				Assert.Equal(RoutineStatus.Running, routine.Act(state));
				Assert.Equal(i * 4, test);
			}
		}
	}
}
