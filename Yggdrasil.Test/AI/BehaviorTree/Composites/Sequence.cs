// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Xunit;
using Yggrasil.Ai.BehaviorTree;
using Yggrasil.Ai.BehaviorTree.Composites;

namespace Yggdrasil.Test.AI.BehaviorTree.Composites
{
	public class SequenceTests
	{
		public int TestInc;

		[Fact]
		public void Sequence_OneRoutine()
		{
			var state = new State();

			var sequence = new Sequence(
				new TestRoutine(this)
			);

			Assert.Equal(RoutineStatus.Success, sequence.Act(state));
			Assert.Equal(1, this.TestInc);
			state.Reset();

			Assert.Equal(RoutineStatus.Success, sequence.Act(state));
			Assert.Equal(2, this.TestInc);
			state.Reset();

			Assert.Equal(RoutineStatus.Success, sequence.Act(state));
			state.Reset();
			Assert.Equal(RoutineStatus.Success, sequence.Act(state));
			state.Reset();
			Assert.Equal(RoutineStatus.Success, sequence.Act(state));
			state.Reset();
			Assert.Equal(5, this.TestInc);
		}

		[Fact]
		public void Sequence_MultipleRoutines()
		{
			var state = new State();

			var sequence = new Sequence(
				new TestRoutine(this),
				new TestRoutine(this),
				new TestRoutine(this)
			);

			Assert.Equal(RoutineStatus.Running, sequence.Act(state));
			Assert.Equal(RoutineStatus.Running, sequence.Act(state));
			Assert.Equal(RoutineStatus.Success, sequence.Act(state));
			state.Reset();
			Assert.Equal(3, this.TestInc);

			this.TestInc = 0;
			Assert.Equal(RoutineStatus.Running, sequence.Act(state));
			Assert.Equal(RoutineStatus.Running, sequence.Act(state));
			Assert.Equal(RoutineStatus.Success, sequence.Act(state));
			state.Reset();
			Assert.Equal(RoutineStatus.Running, sequence.Act(state));
			Assert.Equal(RoutineStatus.Running, sequence.Act(state));
			Assert.Equal(RoutineStatus.Success, sequence.Act(state));
			state.Reset();
			Assert.Equal(6, this.TestInc);
		}

		[Fact]
		public void Sequence_Fail()
		{
			var state = new State();

			var sequence = new Sequence(
				new TestRoutine(this),
				new TestRoutine(this),
				new TestRoutine(this)
			);

			this.TestInc = 6;

			Assert.Equal(RoutineStatus.Running, sequence.Act(state));
			Assert.Equal(RoutineStatus.Running, sequence.Act(state));
			Assert.Equal(RoutineStatus.Success, sequence.Act(state));
			state.Reset();
			Assert.Equal(RoutineStatus.Running, sequence.Act(state));
			Assert.Equal(RoutineStatus.Failure, sequence.Act(state));
			Assert.Equal(11, this.TestInc);
			Assert.Equal(RoutineStatus.Failure, sequence.Act(state));
			Assert.Equal(RoutineStatus.Failure, sequence.Act(state));
			Assert.Equal(11, this.TestInc);
		}
	}

	public class TestRoutine : Routine
	{
		public readonly SequenceTests That;

		public TestRoutine(SequenceTests that)
		{
			this.That = that;
		}

		public override RoutineStatus Act(State state)
		{
			this.That.TestInc++;
			if (this.That.TestInc < 11)
				return RoutineStatus.Success;
			else
				return RoutineStatus.Failure;
		}
	}
}
