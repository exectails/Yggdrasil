// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

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
