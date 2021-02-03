namespace Yggrasil.Ai.BehaviorTree.Decorators
{
	/// <summary>
	/// Acts on given routine and never returns Failure.
	/// </summary>
	public class Succeeder : Routine
	{
		/// <summary>
		/// Routine to be run and inverted.
		/// </summary>
		public readonly Routine Routine;

		/// <summary>
		/// Creates new instance of Succeeder routine.
		/// </summary>
		/// <param name="routine"></param>
		public Succeeder(Routine routine)
		{
			this.Routine = routine;
		}

		/// <summary>
		/// Runs the routine once.
		/// </summary>
		/// <param name="state"></param>
		/// <returns></returns>
		public override RoutineStatus Act(State state)
		{
			var result = this.Routine.Act(state);

			switch (result)
			{
				default:
				case RoutineStatus.Running: return RoutineStatus.Running;
				case RoutineStatus.Failure: return RoutineStatus.Success;
				case RoutineStatus.Success: return RoutineStatus.Success;
			}
		}
	}
}
