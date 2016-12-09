// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;

namespace Yggrasil.Ai.Composites
{
	/// <summary>
	/// Runs through all given routines. It fails when they fail, it runs when
	/// they run, and it succeeds once all routines have finished.
	/// </summary>
	public class Sequence : Routine
	{
		private int _routineCount;

		/// <summary>
		/// Routines to run in sequence.
		/// </summary>
		private readonly Routine[] _routines;

		/// <summary>
		/// Creates new instance of Sequence routine.
		/// </summary>
		/// <param name="routines"></param>
		public Sequence(params Routine[] routines)
		{
			_routines = routines;
			_routineCount = routines.Length;
		}

		/// <summary>
		/// Runs routine once.
		/// </summary>
		/// <param name="state"></param>
		/// <returns></returns>
		public override RoutineStatus Act(State state)
		{
			if (_routineCount == 0)
				return RoutineStatus.Success;

			var routineState = state.GetRoutineState<SequenceRoutineState>(this.Id);
			routineState.SetRoutineIds(_routines);

			var result = _routines[routineState.Index].Act(state);
			if (result == RoutineStatus.Success)
			{
				if (++routineState.Index == _routineCount)
					return RoutineStatus.Success;
			}
			else if (result == RoutineStatus.Failure)
			{
				return RoutineStatus.Failure;
			}

			return RoutineStatus.Running;
		}
	}

	/// <summary>
	/// State for Sequence routine, storing which routine is currently
	/// active.
	/// </summary>
	public class SequenceRoutineState : IRoutineState
	{
		private HashSet<long> _routineIds;

		/// <summary>
		/// Routine that is currently run.
		/// </summary>
		public int Index;

		/// <summary>
		/// Returns true if routine ids were set.
		/// </summary>
		public bool AreRoutineIdsSet { get { return _routineIds != null; } }

		/// <summary>
		/// Stores the sequence's routine's ids, to be able to reset them.
		/// Run after creation to set up state.
		/// </summary>
		/// <param name="routines"></param>
		public void SetRoutineIds(IEnumerable<Routine> routines)
		{
			if (_routineIds != null)
				return;

			_routineIds = new HashSet<long>(routines.Select(a => a.Id));
		}

		/// <summary>
		/// Resets sequence, starting over.
		/// </summary>
		/// <param name="state"></param>
		public void Reset(State state)
		{
			if (_routineIds == null)
				throw new InvalidOperationException("Unable to reset Sequence, call SetRoutineIds after creation.");

			this.Index = 0;

			foreach (var id in _routineIds)
				state.Reset(id);
		}
	}
}
