// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;

namespace Yggrasil.Ai.Leafs
{
	/// <summary>
	/// Prints the given text.
	/// </summary>
	public class Print : Routine
	{
		/// <summary>
		/// Text to output.
		/// </summary>
		public readonly string Text;

		/// <summary>
		/// Creates new instance of Print routine.
		/// </summary>
		/// <param name="text"></param>
		public Print(string text)
		{
			this.Text = text;
		}

		/// <summary>
		/// Prints text and a line-break to standard output once.
		/// </summary>
		/// <param name="state"></param>
		/// <returns></returns>
		public override RoutineStatus Act(State state)
		{
			Console.WriteLine(this.Text);

			return RoutineStatus.Success;
		}
	}
}
