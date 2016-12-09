// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Xunit;
using Yggrasil.Ai;
using Yggrasil.Ai.Composites;
using Yggrasil.Ai.Decorators;
using Yggrasil.Ai.Leafs;

namespace Yggdrasil.Test.AI.Leafs
{
	public class RepeaterTests
	{
		[Fact]
		public void Repeater_Times()
		{
			var state = new State();
			var test = 0;

			var sequence = new Sequence(
				new Repeater(2, new Execute(_ => test++))
			);

			for (int i = 1; i <= 100; ++i)
			{
				Assert.Equal(RoutineStatus.Running, sequence.Act(state));
				Assert.Equal(RoutineStatus.Success, sequence.Act(state));
				Assert.Equal(i * 2, test);
				state.Reset();
			}

			for (int i = 1; i <= 100; ++i)
			{
				Assert.Equal(RoutineStatus.Running, sequence.Act(state));
				Assert.Equal(RoutineStatus.Success, sequence.Act(state));
				state.Reset();
				Assert.Equal(RoutineStatus.Running, sequence.Act(state));
				Assert.Equal(RoutineStatus.Success, sequence.Act(state));
				Assert.Equal(200 + i * 4, test);
				state.Reset();
			}
		}

		[Fact]
		public void Repeater_Unlimited()
		{
			var state = new State();
			var test = 0;

			var sequence = new Repeater(
				new Sequence(
					new Execute(_ => test++),
					new Wait(TimeSpan.FromTicks(0)),
					new Execute(_ => test++)
				)
			);

			for (int i = 1; i <= 1234; ++i)
			{
				Assert.Equal(RoutineStatus.Running, sequence.Act(state));
				Assert.Equal(RoutineStatus.Running, sequence.Act(state));
				Assert.Equal(RoutineStatus.Running, sequence.Act(state));
				Assert.Equal(RoutineStatus.Running, sequence.Act(state));
				Assert.Equal(RoutineStatus.Running, sequence.Act(state));
				Assert.Equal(RoutineStatus.Running, sequence.Act(state));
				Assert.Equal(i * 4, test);
			}
		}
	}
}
