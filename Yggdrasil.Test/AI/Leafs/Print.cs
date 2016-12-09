// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.IO;
using System.Text;
using Xunit;
using Yggrasil.Ai;
using Yggrasil.Ai.Composites;
using Yggrasil.Ai.Leafs;

namespace Yggdrasil.Test.AI.Leafs
{
	public class PrintTests
	{
		[Fact]
		public void Print()
		{
			var state = new State();
			var test = "foobar";

			var sequence = new Sequence(
				new Print(test)
			);

			var result = "";

			using (var stream = new MemoryStream())
			{
				using (var sw = new StreamWriter(stream))
				{
					Console.SetOut(sw);

					for (int i = 0; i < 10; ++i)
					{
						Assert.Equal(RoutineStatus.Success, sequence.Act(state));
						state.Reset();

						result += test + Environment.NewLine;
					}
				}

				Assert.Equal(result, Encoding.UTF8.GetString(stream.ToArray()));
			}
		}
	}
}
