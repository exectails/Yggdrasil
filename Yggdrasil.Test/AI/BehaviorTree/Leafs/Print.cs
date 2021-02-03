using System;
using System.IO;
using System.Text;
using Xunit;
using Yggrasil.Ai.BehaviorTree;
using Yggrasil.Ai.BehaviorTree.Leafs;

namespace Yggdrasil.Test.AI.BehaviorTree.Leafs
{
	public class PrintTests
	{
		[Fact]
		public void Print()
		{
			var state = new State();
			var test = "foobar";

			var routine = new Print(test);

			var result = "";

			using (var stream = new MemoryStream())
			{
				using (var sw = new StreamWriter(stream))
				{
					var cout = Console.Out;
					Console.SetOut(sw);

					for (var i = 0; i < 10; ++i)
					{
						Assert.Equal(RoutineStatus.Success, routine.Act(state));
						state.Reset();

						result += test + Environment.NewLine;
					}

					Console.SetOut(cout);
				}

				Assert.Equal(result, Encoding.UTF8.GetString(stream.ToArray()));
			}
		}
	}
}
