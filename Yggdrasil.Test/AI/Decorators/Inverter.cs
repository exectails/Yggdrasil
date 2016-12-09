// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Xunit;
using Yggrasil.Ai;
using Yggrasil.Ai.Composites;
using Yggrasil.Ai.Decorators;

namespace Yggdrasil.Test.AI.Decorators
{
	public class InverterTests
	{
		[Fact]
		public void Inverter()
		{
			var state = new State();
			var test = 0;

			var sequence = new Sequence(
				new Inverter(new Conditional((_) => test < 50))
			);

			for (; test < 50; ++test)
			{
				Assert.Equal(RoutineStatus.Failure, sequence.Act(state));
				state.Reset();
			}

			test++;
			Assert.Equal(RoutineStatus.Success, sequence.Act(state));
			state.Reset();

			Assert.Equal(RoutineStatus.Success, sequence.Act(state));
			state.Reset();

			test = 1;
			Assert.Equal(RoutineStatus.Failure, sequence.Act(state));
			state.Reset();
		}
	}
}
