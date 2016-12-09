// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Yggrasil.Ai.Decorators
{
	/// <summary>
	/// Repeats the given routine for the given amount of times.
	/// </summary>
	public class Repeater : Routine
	{
		/// <summary>
		/// Number of times the routine is repeated.
		/// </summary>
		public readonly int Repeats;

		/// <summary>
		/// Routine to be repeated.
		/// </summary>
		public readonly Routine Routine;

		/// <summary>
		/// Creates new instance of Repeater routine.
		/// </summary>
		/// <param name="repeats"></param>
		/// <param name="routine"></param>
		public Repeater(int repeats, Routine routine)
		{
			this.Repeats = repeats;
			this.Routine = routine;
		}

		/// <summary>
		/// Runs routine once.
		/// </summary>
		/// <param name="state"></param>
		/// <returns></returns>
		public override RoutineStatus Act(State state)
		{
			if (this.Repeats <= 0 || this.Routine == null)
				return RoutineStatus.Success;

			var result = this.Routine.Act(state);
			if (result != RoutineStatus.Success)
				return result;

			state.Reset(this.Routine.Id);

			var routineState = state.GetRoutineState<RepeaterRoutineState>(this.Id);
			if (++routineState.I < this.Repeats)
				return RoutineStatus.Running;

			return RoutineStatus.Success;
		}
	}

	/// <summary>
	/// State for Repeater routine, storing the number of times the routine
	/// has been repeated.
	/// </summary>
	public class RepeaterRoutineState : IRoutineState
	{
		/// <summary>
		/// Times the routine has been executed.
		/// </summary>
		public int I;

		/// <summary>
		/// Resets times the routine has been executed.
		/// </summary>
		/// <param name="state"></param>
		public void Reset(State state)
		{
			this.I = 0;
		}
	}
}
