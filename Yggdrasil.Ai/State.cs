// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;

namespace Yggrasil.Ai
{
	/// <summary>
	/// State can traverse routines.
	/// </summary>
	public class State
	{
		private Dictionary<long, IRoutineState> _routineStates = new Dictionary<long, IRoutineState>();

		/// <summary>
		/// Returns the routine state with the given id. If it doesn't exist
		/// yet, it's created.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="id"></param>
		/// <returns></returns>
		public T GetRoutineState<T>(long id) where T : IRoutineState, new()
		{
			if (_routineStates.ContainsKey(id))
			{
				return (T)_routineStates[id];
			}
			else
			{
				return (T)(_routineStates[id] = new T());
			}
		}

		/// <summary>
		/// Resets all routine states of this state.
		/// </summary>
		public void Reset()
		{
			foreach (var routineState in _routineStates.Values)
				routineState.Reset(this);
		}

		/// <summary>
		/// Resets the routine state with the given id.
		/// </summary>
		/// <param name="id"></param>
		public void Reset(long id)
		{
			if (_routineStates.ContainsKey(id))
				_routineStates[id].Reset(this);
		}
	}
}
