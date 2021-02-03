using System;

namespace Yggrasil.Ai.BehaviorTree.Leafs
{
	/// <summary>
	/// Executes the given action.
	/// </summary>
	public class Execute : Routine
	{
		/// <summary>
		/// Action to execute.
		/// </summary>
		public readonly Action<State> Action;

		/// <summary>
		/// Creates new instance of Execute routine.
		/// </summary>
		/// <param name="action"></param>
		public Execute(Action<State> action)
		{
			this.Action = action;
		}

		/// <summary>
		/// Runs action once.
		/// </summary>
		/// <param name="state"></param>
		/// <returns></returns>
		public override RoutineStatus Act(State state)
		{
			this.Action(state);
			return RoutineStatus.Success;
		}
	}
}
