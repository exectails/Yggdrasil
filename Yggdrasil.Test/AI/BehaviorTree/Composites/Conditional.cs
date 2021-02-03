using Xunit;
using Yggrasil.Ai.BehaviorTree;
using Yggrasil.Ai.BehaviorTree.Composites;

namespace Yggdrasil.Test.AI.BehaviorTree.Composites
{
	public class ConditionalTests
	{
		[Fact]
		public void Conditional()
		{
			var state = new State();
			var test = 0;

			var routine = new Conditional((_) => test < 50);

			for (; test < 50; ++test)
			{
				Assert.Equal(RoutineStatus.Success, routine.Act(state));
				state.Reset();
			}

			test++;
			Assert.Equal(RoutineStatus.Failure, routine.Act(state));
			state.Reset();

			Assert.Equal(RoutineStatus.Failure, routine.Act(state));
			state.Reset();

			test = 1;
			Assert.Equal(RoutineStatus.Success, routine.Act(state));
			state.Reset();
		}
	}
}
