// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;

namespace Yggrasil.Ai.Composites
{
	/// <summary>
	/// Returns either Success or Failure, depending on the result of the
	/// given function.
	/// </summary>
	public class Conditional : Routine
	{
		/// <summary>
		/// Function to base result on.
		/// </summary>
		public readonly Func<State, bool> Func;

		/// <summary>
		/// Creates new instance of Conditional routine.
		/// </summary>
		/// <param name="func"></param>
		public Conditional(Func<State, bool> func)
		{
			this.Func = func;
		}

		/// <summary>
		/// Runs function once and returns status based on its result.
		/// </summary>
		/// <param name="state"></param>
		/// <returns></returns>
		public override RoutineStatus Act(State state)
		{
			var result = this.Func(state);
			return (result ? RoutineStatus.Success : RoutineStatus.Failure);
		}
	}
}
