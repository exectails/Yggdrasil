// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;

namespace Yggrasil.Ai.Leafs
{
	/// <summary>
	/// Returns running until the given time span is over.
	/// </summary>
	public class Wait : Routine
	{
		/// <summary>
		/// Amount of the time the routine waits.
		/// </summary>
		public readonly TimeSpan TimeSpan;

		/// <summary>
		/// Creates new instance Wait routine.
		/// </summary>
		/// <param name="timeSpan"></param>
		public Wait(TimeSpan timeSpan)
		{
			this.TimeSpan = timeSpan;
		}

		/// <summary>
		/// Runs routine once, returns Running until the time span is over.
		/// </summary>
		/// <param name="state"></param>
		/// <returns></returns>
		public override RoutineStatus Act(State state)
		{
			var now = DateTime.Now;

			var routineState = state.GetRoutineState<WaitRoutineState>(this.Id);
			if (routineState.IsEmpty)
				routineState.Until = now.Add(this.TimeSpan);

			if (now < routineState.Until)
				return RoutineStatus.Running;

			return RoutineStatus.Success;
		}
	}

	/// <summary>
	/// State for Wait routine, storing the time until which it should run.
	/// </summary>
	public class WaitRoutineState : IRoutineState
	{
		/// <summary>
		/// Time until which the routine should run.
		/// </summary>
		public DateTime Until;

		/// <summary>
		/// Returns true if Until hasn't been set yet.
		/// </summary>
		public bool IsEmpty { get { return (this.Until == DateTime.MinValue); } }

		/// <summary>
		/// Resets routine, starting over.
		/// </summary>
		/// <param name="state"></param>
		public void Reset(State state)
		{
			this.Until = DateTime.MinValue;
		}
	}
}
