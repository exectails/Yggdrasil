// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Threading;

namespace Yggrasil.Ai.BehaviorTree
{
	/// <summary>
	/// Base class for routines.
	/// </summary>
	public abstract class Routine
	{
		private static long _uniqueId;

		/// <summary>
		/// The routine's unique id.
		/// </summary>
		public long Id;

		/// <summary>
		/// Initializes routine.
		/// </summary>
		public Routine()
		{
			this.Id = Interlocked.Increment(ref _uniqueId);
		}

		/// <summary>
		/// Called to run the method.
		/// </summary>
		/// <param name="state"></param>
		/// <returns></returns>
		public abstract RoutineStatus Act(State state);
	}

	/// <summary>
	/// Status of the routine after Act.
	/// </summary>
	public enum RoutineStatus
	{
		/// <summary>
		/// Routine ended successfully.
		/// </summary>
		Success,

		/// <summary>
		/// Routine failed to end successfully.
		/// </summary>
		Failure,

		/// <summary>
		/// Routine is currently running.
		/// </summary>
		Running,
	}
}
