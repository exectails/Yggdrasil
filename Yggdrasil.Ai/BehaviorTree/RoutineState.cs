namespace Yggrasil.Ai.BehaviorTree
{
	/// <summary>
	/// Interface for routine states.
	/// </summary>
	public interface IRoutineState
	{
		/// <summary>
		/// Resets routine, bringing it back to its starting state.
		/// </summary>
		/// <param name="state"></param>
		void Reset(State state);
	}
}
